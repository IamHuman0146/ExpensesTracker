using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class ViewFinancialAchievementsPage : Page
    {
        public ViewFinancialAchievementsPage()
        {
            InitializeComponent();
            LoadAchievements();
        }

        private void LoadAchievements()
        {
            string filePath = $"Goals_{UserSession.CurrentUserId}.json";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("No financial goals found.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                GoalsDataGrid.ItemsSource = null;
                return;
            }

            try
            {
                string data = File.ReadAllText(filePath);
                var goals = JsonSerializer.Deserialize<List<Goal>>(data) ?? new List<Goal>();

                var achievementData = goals.Select(goal => new
                {
                    Name = goal.Name,
                    Progress = CalculateProgress(goal.Amount, goal.StartDate, goal.Currency),
                    ProgressText = CalculateProgress(goal.Amount, goal.StartDate, goal.Currency) >= 100
                        ? "Achieved"
                        : $"{CalculateProgress(goal.Amount, goal.StartDate, goal.Currency):F2}% completed"
                }).ToList();

                GoalsDataGrid.ItemsSource = achievementData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading achievements: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal CalculateProgress(decimal goalAmount, DateTime startDate, string currency)
        {
            int monthsPassed = GetMonthsSince(startDate);
            decimal monthlyIncome = UserSession.Income;

            // Convert monthly income to the goal's currency if necessary
            decimal savedAmount = monthsPassed * ConvertCurrency(monthlyIncome, UserSession.CurrencySymbol, currency);

            // Calculate progress percentage and cap it at 100%
            return Math.Min((savedAmount / goalAmount) * 100, 100);
        }

        private decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            const decimal USD_TO_IDR_RATE = 14000; // Example conversion rate: $1 = Rp14,000

            if (fromCurrency == "$" && toCurrency == "Rp")
                return amount * USD_TO_IDR_RATE;
            else if (fromCurrency == "Rp" && toCurrency == "$")
                return amount / USD_TO_IDR_RATE;

            return amount; // No conversion needed if currencies match
        }

        private int GetMonthsSince(DateTime startDate)
        {
            DateTime today = DateTime.Now;
            return Math.Max((today.Year - startDate.Year) * 12 + (today.Month - startDate.Month), 0);
        }

        private void DeleteGoal_Click(object sender, RoutedEventArgs e)
        {
            if (GoalsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a goal to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string filePath = $"Goals_{UserSession.CurrentUserId}.json";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("No goals found to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string data = File.ReadAllText(filePath);
                var goals = JsonSerializer.Deserialize<List<Goal>>(data) ?? new List<Goal>();

                // Get the selected goal
                dynamic selectedGoal = GoalsDataGrid.SelectedItem;
                string selectedGoalName = selectedGoal.Name;

                // Remove the goal with the matching name
                goals.RemoveAll(g => g.Name == selectedGoalName);

                // Save updated goals back to the file
                File.WriteAllText(filePath, JsonSerializer.Serialize(goals));

                MessageBox.Show("Goal deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload the achievements
                LoadAchievements();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting goal: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Goal class for deserialization
    public class Goal
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public string Currency { get; set; }
    }
}
