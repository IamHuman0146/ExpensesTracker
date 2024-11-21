using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ExpensesTracker
{
    public partial class SetFinancialAchievementsPage : Page
    {
        private bool isUpdatingTextBox = false;

        public SetFinancialAchievementsPage()
        {
            InitializeComponent();

            StartDatePicker.SelectedDate = DateTime.Now;
            CurrencyComboBox.SelectedIndex = UserSession.CurrencySymbol == "Rp" ? 0 : 1;

            // Initially set budget to "Loading..."
            BudgetLabel.Text = "Loading...";
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(UpdateBudgetLabel));
        }

        private void UpdateBudgetLabel()
        {
            try
            {
                BudgetLabel.Text = "Loading..."; // Show loading status
                decimal remainingIncome = ExpenseDataStore.RemainingIncome; // Fetch remaining income
                int monthsPassed = GetMonthsSinceGoalStart();

                decimal totalBudget = remainingIncome + (monthsPassed * ExpenseDataStore.Income);
                BudgetLabel.Text = FormatCurrency(totalBudget); // Format currency
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating budget: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetMonthsSinceGoalStart()
        {
            DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Now;
            DateTime today = DateTime.Now;

            return Math.Max((today.Year - startDate.Year) * 12 + (today.Month - startDate.Month), 0);
        }

        private string FormatCurrency(decimal amount)
        {
            string symbol = UserSession.CurrencySymbol;
            string separator = symbol == "Rp" ? "." : ",";

            return symbol == "Rp"
                ? $"{symbol}{amount:N0}".Replace(",", separator)
                : $"{symbol}{amount:N0}".Replace(".", separator);
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedCurrency = (CurrencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            UserSession.CurrencySymbol = selectedCurrency == "Dollar" ? "$" : "Rp";

            // Reformat the BudgetLabel and GoalAmountTextBox based on currency
            UpdateBudgetLabel();
            GoalAmountTextBox_TextChanged(GoalAmountTextBox, null);
        }

        private void GoalAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTextBox) return;

            isUpdatingTextBox = true;
            try
            {
                if (decimal.TryParse(GoalAmountTextBox.Text.Replace(".", "").Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal amount))
                {
                    string formatted = FormatCurrency(amount);
                    GoalAmountTextBox.Text = formatted;
                    GoalAmountTextBox.CaretIndex = GoalAmountTextBox.Text.Length;
                }
            }
            catch
            {
                // Ignore errors
            }
            finally
            {
                isUpdatingTextBox = false;
            }
        }

        private void SetGoalButton_Click(object sender, RoutedEventArgs e)
        {
            string goalName = GoalNameTextBox.Text.Trim();

            if (!decimal.TryParse(GoalAmountTextBox.Text.Replace(".", "").Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal goalAmount) || goalAmount <= 0)
            {
                MessageBox.Show("Please enter a valid goal amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Now;

            var goal = new Goal
            {
                Name = goalName,
                Amount = goalAmount,
                StartDate = startDate,
                Currency = UserSession.CurrencySymbol
            };

            string filePath = $"Goals_{UserSession.CurrentUserId}.json";
            var goals = File.Exists(filePath)
                ? JsonSerializer.Deserialize<List<Goal>>(File.ReadAllText(filePath)) ?? new List<Goal>()
                : new List<Goal>();

            goals.Add(goal);
            File.WriteAllText(filePath, JsonSerializer.Serialize(goals));

            MessageBox.Show("Goal has been set successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            GoalNameTextBox.Clear();
            GoalAmountTextBox.Clear();
            StartDatePicker.SelectedDate = DateTime.Now;

            UpdateBudgetLabel(); // Refresh budget after setting the goal
        }
    }

    
}
