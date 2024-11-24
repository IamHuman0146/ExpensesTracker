using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class SetFinancialAchievementsPage : Page
    {
        private bool isUpdatingTextBox = false;

        public SetFinancialAchievementsPage()
        {
            InitializeComponent();
        }

        private string FormatCurrency(decimal amount)
        {
            string symbol = ExpenseDataStore.CurrencySymbol;
            string separator = symbol == "Rp" ? "." : ",";

            return symbol == "Rp"
                ? $"{symbol}{amount:N0}".Replace(",", separator)
                : $"{symbol}{amount:N0}".Replace(".", separator);
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

            if (string.IsNullOrWhiteSpace(goalName))
            {
                MessageBox.Show("Please enter a goal name.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(GoalAmountTextBox.Text.Replace(".", "").Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal goalAmount) || goalAmount <= 0)
            {
                MessageBox.Show("Please enter a valid goal amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime? startDate = StartDatePicker.SelectedDate;
            if (startDate == null)
            {
                MessageBox.Show("Please select a start date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var goal = new Goal
            {
                Name = goalName,
                Amount = goalAmount,
                StartDate = startDate.Value,
                Currency = ExpenseDataStore.CurrencySymbol
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
            StartDatePicker.SelectedDate = null;

            // Notify other pages to update progress
            ExpenseDataStore.NotifyBudgetUpdated();
        }
    }

    public class Goal
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public string Currency { get; set; }
    }
}
