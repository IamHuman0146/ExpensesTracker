using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class ViewFinancialAchievementsPage : Page
    {
        private string primaryGoalName;
        private Dictionary<string, string> goalStatuses = new Dictionary<string, string>();
        private readonly List<string> statusOptions = new List<string> { "Spent", "Kept" };
        private string goalsFilePath => $"Goals_{UserSession.CurrentUserId}.json";
        private string goalStatusesFilePath => $"GoalStatuses_{UserSession.CurrentUserId}.json";

        public ViewFinancialAchievementsPage()
        {
            InitializeComponent();

            // Subscribe to budget updates
            ExpenseDataStore.BudgetUpdated += OnBudgetUpdated;

            LoadPrimaryGoal();
            LoadGoalStatuses();
            LoadAchievements();

            // Update budget label when page is loaded
            UpdateBudgetLabel();
        }

        private void LoadPrimaryGoal()
        {
            string primaryGoalFilePath = $"PrimaryGoal_{UserSession.CurrentUserId}.json";
            if (File.Exists(primaryGoalFilePath))
                primaryGoalName = File.ReadAllText(primaryGoalFilePath);
        }

        private void SavePrimaryGoal()
        {
            File.WriteAllText($"PrimaryGoal_{UserSession.CurrentUserId}.json", primaryGoalName ?? string.Empty);
        }

        private void LoadGoalStatuses()
        {
            if (File.Exists(goalStatusesFilePath))
            {
                string data = File.ReadAllText(goalStatusesFilePath);
                goalStatuses = JsonSerializer.Deserialize<Dictionary<string, string>>(data) ?? new Dictionary<string, string>();
            }
        }

        private void SaveGoalStatuses()
        {
            File.WriteAllText(goalStatusesFilePath, JsonSerializer.Serialize(goalStatuses));
        }

        private void SaveAchievements()
        {
            // Ensure goalStatuses dictionary is updated before saving
            if (AchievedGoalsDataGrid.ItemsSource is List<AchievedGoalData> achievedGoals)
            {
                foreach (var goal in achievedGoals)
                {
                    goalStatuses[goal.Name] = goal.Status;
                }
            }

            // Save the updated statuses to a file
            SaveGoalStatuses();
            ExpenseDataStore.NotifyBudgetUpdated();
        }

        private void LoadAchievements()
        {
            if (!File.Exists(goalsFilePath))
            {
                GoalsDataGrid.ItemsSource = null;
                AchievedGoalsDataGrid.ItemsSource = null;
                UpdateBudgetLabel();
                return;
            }

            string data = File.ReadAllText(goalsFilePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(data) ?? new List<Goal>();
            decimal currentBudget = ExpenseDataStore.CalculatedBudget;

            decimal primaryGoalBudget = 0;
            decimal sharedBudget = currentBudget;

            if (!string.IsNullOrWhiteSpace(primaryGoalName))
            {
                var primaryGoal = goals.FirstOrDefault(g => g.Name == primaryGoalName);
                if (primaryGoal != null)
                {
                    primaryGoalBudget = Math.Min(currentBudget * 0.7m, primaryGoal.Amount);
                    sharedBudget = currentBudget - primaryGoalBudget;
                }
            }

            var completedGoals = goals.Where(g => goalStatuses.ContainsKey(g.Name)).ToList();
            var inProgressGoals = goals.Except(completedGoals).ToList();

            GoalsDataGrid.ItemsSource = inProgressGoals.Select(goal =>
            {
                decimal allocatedBudget = primaryGoalName == goal.Name ? primaryGoalBudget : sharedBudget / inProgressGoals.Count;
                decimal progress = Math.Min((allocatedBudget / goal.Amount) * 100, 100);
                return new AchievementData
                {
                    Name = goal.Name,
                    AmountSet = $"{goal.Currency}{goal.Amount:N2}",
                    StartDate = goal.StartDate.ToString("yyyy-MM-dd"),
                    Progress = progress,
                    ProgressText = $"{progress:F2}%",
                    IsPrimary = goal.Name == primaryGoalName
                };
            }).ToList();

            AchievedGoalsDataGrid.ItemsSource = completedGoals.Select(goal => new AchievedGoalData
            {
                Name = goal.Name,
                AmountSet = $"{goal.Currency}{goal.Amount:N2}",
                StartDate = goal.StartDate.ToString("yyyy-MM-dd"),
                Status = goalStatuses.ContainsKey(goal.Name) ? goalStatuses[goal.Name] : "Kept"
            }).ToList();

            UpdateBudgetLabel();
        }

        private void UpdateBudgetLabel()
        {
            BudgetLabel.Text = $"{ExpenseDataStore.CurrencySymbol}{ExpenseDataStore.CalculatedBudget:N0}";
        }

        private void OnBudgetUpdated()
        {
            UpdateBudgetLabel();
            LoadAchievements();
        }

        private void SetPrimaryGoal_Click(object sender, RoutedEventArgs e)
        {
            var selectedGoal = GoalsDataGrid.SelectedItem as AchievementData;
            if (selectedGoal == null) return;

            primaryGoalName = selectedGoal.Name;
            SavePrimaryGoal();
            LoadAchievements();
        }

        private void DeleteGoal_Click(object sender, RoutedEventArgs e)
        {
            var selectedGoal = GoalsDataGrid.SelectedItem as AchievementData;
            if (selectedGoal == null) return;

            if (!File.Exists(goalsFilePath)) return;

            string data = File.ReadAllText(goalsFilePath);
            var goals = JsonSerializer.Deserialize<List<Goal>>(data) ?? new List<Goal>();
            goals.RemoveAll(g => g.Name == selectedGoal.Name);
            if (primaryGoalName == selectedGoal.Name) primaryGoalName = null;

            File.WriteAllText(goalsFilePath, JsonSerializer.Serialize(goals));
            SavePrimaryGoal();
            LoadAchievements();
        }

        private void AchievedGoalsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedGoal = AchievedGoalsDataGrid.SelectedItem as AchievedGoalData;
            if (selectedGoal == null) return;

            MessageBoxResult result = MessageBox.Show($"Do you want to mark this goal as 'Spent' or 'Kept'?\n\n" +
                                                      $"Goal: {selectedGoal.Name}\nAmount: {selectedGoal.AmountSet}",
                                                      "Mark Goal", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                decimal amount = decimal.Parse(selectedGoal.AmountSet.Replace("$", "").Replace(",", ""));
                ExpenseDataStore.ApplyGoalOutcome(selectedGoal.Name, amount, "Spent");
                selectedGoal.Status = "Spent";
                SaveAchievements();
            }
            else if (result == MessageBoxResult.No)
            {
                selectedGoal.Status = "Kept";
                SaveAchievements();
            }
            ExpenseDataStore.NotifyBudgetUpdated();
        }

        private void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Use this page to manage your financial goals and achievements.", "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class AchievementData
    {
        public string Name { get; set; }
        public string AmountSet { get; set; }
        public string StartDate { get; set; }
        public decimal Progress { get; set; }
        public string ProgressText { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class AchievedGoalData
    {
        public string Name { get; set; }
        public string AmountSet { get; set; }
        public string StartDate { get; set; }
        public string Status { get; set; }
    }
}
