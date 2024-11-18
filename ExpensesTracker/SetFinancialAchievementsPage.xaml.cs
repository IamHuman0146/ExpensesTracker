using System;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class SetFinancialAchievementsPage : Page
    {
        public SetFinancialAchievementsPage()
        {
            InitializeComponent();
        }

        private void SetGoalButton_Click(object sender, RoutedEventArgs e)
        {
            string goalName = GoalNameTextBox.Text;
            if (decimal.TryParse(GoalAmountTextBox.Text.Replace(",", ""), out decimal goalAmount) &&
                GoalDeadlinePicker.SelectedDate.HasValue)
            {
                DateTime goalDeadline = GoalDeadlinePicker.SelectedDate.Value;

                ExpenseDataStore.FinancialGoal = goalAmount;
                ExpenseDataStore.GoalName = goalName;
                ExpenseDataStore.GoalDeadline = goalDeadline;

                MessageBox.Show("Financial goal set successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GoalAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderTextBlock.Visibility = string.IsNullOrEmpty(GoalAmountTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
