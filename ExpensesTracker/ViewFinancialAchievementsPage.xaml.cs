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
        private string goalsFilePath => $"Goals_{UserSession.CurrentUserId}.json";
        private string goalStatusesFilePath => $"GoalStatuses_{UserSession.CurrentUserId}.json";

        public ViewFinancialAchievementsPage()
        {
            InitializeComponent();

            // Subscribe to the Loaded event
            this.Loaded += ViewFinancialAchievementsPage_Loaded;

            // Subscribe to budget updates
            ExpenseDataStore.BudgetUpdated += OnBudgetUpdated;
        }

        ~ViewFinancialAchievementsPage()
        {
            // Unsubscribe to avoid memory leaks
            ExpenseDataStore.BudgetUpdated -= OnBudgetUpdated;
        }

        private void ViewFinancialAchievementsPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Recalculate the budget when the page loads
                ExpenseDataStore.UpdateCalculatedBudget();

                primaryGoalName = null;
                goalStatuses.Clear();

                LoadPrimaryGoal();
                LoadGoalStatuses();
                LoadAchievements();
                UpdateBudgetLabel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during page load: {ex.Message}");
            }
        }


        private void SetPrimaryGoal_Click(object sender, RoutedEventArgs e)
        {
            var selectedGoal = GoalsDataGrid.SelectedItem as AchievementData;
            if (selectedGoal == null)
            {
                MessageBox.Show("Please select a goal to set as primary.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            primaryGoalName = selectedGoal.Name;
            SavePrimaryGoal();

            MessageBox.Show($"'{primaryGoalName}' has been set as the primary goal.", "Primary Goal Set", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadAchievements();
        }

        private void ChangeGoalStatus_Click(object sender, RoutedEventArgs e)
        {
            var selectedGoal = AchievedGoalsDataGrid.SelectedItem as AchievedGoalData;

            if (selectedGoal == null)
            {
                MessageBox.Show("Please select a completed goal to change its status.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var goals = JsonSerializer.Deserialize<List<Goal>>(File.ReadAllText(goalsFilePath));
            var selectedGoalData = goals.FirstOrDefault(g => g.Name == selectedGoal.Name);

            if (selectedGoalData == null)
            {
                MessageBox.Show("Goal not found in the saved data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Change the status of this goal?\n\nGoal: {selectedGoal.Name}\n\n" +
                                                      "Choose 'Yes' to mark it as Spent or 'No' to mark it as Kept.",
                                                      "Change Goal Status",
                                                      MessageBoxButton.YesNoCancel,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Mark as "Spent" and deduct budget
                try
                {
                    if (goalStatuses.TryGetValue(selectedGoal.Name, out string currentStatus) && currentStatus == "Kept")
                    {
                        // If previously "Kept," deduct the amount from the budget
                        ExpenseDataStore.CalculatedBudget -= selectedGoalData.Amount;
                        BudgetHelper.SaveBudget(ExpenseDataStore.UserID, ExpenseDataStore.CalculatedBudget);
                    }

                    goalStatuses[selectedGoal.Name] = "Spent";
                    SaveGoalStatuses();

                    // Refresh UI
                    UpdateBudgetLabel();
                    LoadAchievements();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while updating the goal status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (result == MessageBoxResult.No)
            {
                // Mark as "Kept" and restore budget if previously "Spent"
                try
                {
                    if (goalStatuses.TryGetValue(selectedGoal.Name, out string currentStatus) && currentStatus == "Spent")
                    {
                        // If previously "Spent," restore the amount to the budget
                        ExpenseDataStore.CalculatedBudget += selectedGoalData.Amount;
                        BudgetHelper.SaveBudget(ExpenseDataStore.UserID, ExpenseDataStore.CalculatedBudget);
                    }

                    goalStatuses[selectedGoal.Name] = "Kept";
                    SaveGoalStatuses();

                    // Refresh UI
                    UpdateBudgetLabel();
                    LoadAchievements();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while keeping the goal status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




        private void DeleteCompletedGoal_Click(object sender, RoutedEventArgs e)
        {
            // Check if a goal is selected from either GoalsDataGrid or AchievedGoalsDataGrid
            AchievementData selectedInProgressGoal = GoalsDataGrid.SelectedItem as AchievementData;
            AchievedGoalData selectedCompletedGoal = AchievedGoalsDataGrid.SelectedItem as AchievedGoalData;

            if (selectedInProgressGoal == null && selectedCompletedGoal == null)
            {
                MessageBox.Show("Please select a goal to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string goalName = selectedInProgressGoal?.Name ?? selectedCompletedGoal?.Name;

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete this goal?\n\nGoal: {goalName}",
                                                      "Delete Goal",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Remove from goals list and save to file
                    if (File.Exists(goalsFilePath))
                    {
                        string data = File.ReadAllText(goalsFilePath);
                        var goals = JsonSerializer.Deserialize<List<Goal>>(data) ?? new List<Goal>();
                        goals.RemoveAll(g => g.Name == goalName);
                        File.WriteAllText(goalsFilePath, JsonSerializer.Serialize(goals));
                    }

                    // Remove from goalStatuses and save to file
                    if (goalStatuses.ContainsKey(goalName))
                    {
                        goalStatuses.Remove(goalName);
                        SaveGoalStatuses();
                    }

                    // Reload UI
                    LoadAchievements();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while deleting the goal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void ShowHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("If a goal is marked as Primary, 70% of your budget is allocated to it. The remaining budget is shared equally among other goals.",
                            "Help",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void UpdateBudgetLabel()
        {
            decimal calculatedBudget = BudgetHelper.LoadBudget(ExpenseDataStore.UserID);
            BudgetLabel.Text = $"{ExpenseDataStore.CurrencySymbol}{calculatedBudget:N0}";
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

        private void OnBudgetUpdated()
        {
            try
            {
                UpdateBudgetLabel();
                LoadAchievements();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during budget update handling: {ex.Message}");
            }
        }

        private void LoadAchievements()
        {
            try
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
                decimal currentBudget = ExpenseDataStore.InitialBudget;

                // Ensure only spent goals reduce the budget
                decimal spentOnCompletedGoals = goals
                    .Where(g => goalStatuses.TryGetValue(g.Name, out string status) && status == "Spent")
                    .Sum(g => g.Amount);

                currentBudget -= spentOnCompletedGoals;
                ExpenseDataStore.CalculatedBudget = currentBudget;

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

                int otherGoalsCount = goals.Count(g => g.Name != primaryGoalName);
                decimal otherGoalBudget = otherGoalsCount > 0 ? sharedBudget / otherGoalsCount : 0;

                var inProgressGoals = new List<AchievementData>();
                var completedGoals = new List<AchievedGoalData>();

                foreach (var goal in goals)
                {
                    decimal allocatedBudget = goal.Name == primaryGoalName ? primaryGoalBudget : otherGoalBudget;
                    decimal progress = Math.Min((allocatedBudget / goal.Amount) * 100, 100);

                    // Default completed goal status to "Kept" if not explicitly marked
                    if (progress >= 100 || (goalStatuses.TryGetValue(goal.Name, out string status) && status == "Spent"))
                    {
                        if (!goalStatuses.ContainsKey(goal.Name))
                        {
                            goalStatuses[goal.Name] = "Kept"; // Default to "Kept" if no status is present
                        }

                        completedGoals.Add(new AchievedGoalData
                        {
                            Name = goal.Name,
                            AmountSet = $"{goal.Currency}{goal.Amount:N2}",
                            StartDate = goal.StartDate.ToString("yyyy-MM-dd"),
                            Status = goalStatuses[goal.Name] // Ensure accurate status display
                        });
                    }
                    else
                    {
                        inProgressGoals.Add(new AchievementData
                        {
                            Name = goal.Name,
                            AmountSet = $"{goal.Currency}{goal.Amount:N2}",
                            StartDate = goal.StartDate.ToString("yyyy-MM-dd"),
                            Progress = progress,
                            ProgressText = $"{progress:F2}%",
                            IsPrimary = goal.Name == primaryGoalName
                        });
                    }
                }

                GoalsDataGrid.ItemsSource = inProgressGoals;
                AchievedGoalsDataGrid.ItemsSource = completedGoals;

                SaveGoalStatuses(); // Persist updated statuses (e.g., defaulting to "Kept")
                UpdateBudgetLabel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading achievements: {ex.Message}");
            }
        }


        private void RefreshBudget_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load the latest budget from persistent storage
                ExpenseDataStore.CalculatedBudget = BudgetHelper.LoadBudget(ExpenseDataStore.UserID);

                // Update the budget label
                UpdateBudgetLabel();

                // Reload achievements and update UI
                LoadAchievements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while refreshing the budget: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
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
