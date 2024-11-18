using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ExpensesTracker
{
    public partial class ViewAnalysisPage : Page
    {
        private Expense _selectedExpense;

        public ViewAnalysisPage()
        {
            InitializeComponent();
            ExpensesDataGrid.ItemsSource = ExpenseDataStore.Expenses;

            UpdateIncomeDisplay();
            UpdateRemainingIncomeDisplay();

            // Refresh income and remaining income when expenses change
            ExpenseDataStore.Expenses.CollectionChanged += (s, e) =>
            {
                UpdateRemainingIncomeDisplay();
            };
        }

        private void UpdateIncomeDisplay()
        {
            IncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{ExpenseDataStore.Income:N0}";
        }

        private void UpdateRemainingIncomeDisplay()
        {
            decimal remainingIncome = ExpenseDataStore.RemainingIncome;
            RemainingIncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{remainingIncome:N0}";

            // Set the color based on value
            RemainingIncomeTextBlock.Foreground = remainingIncome < 0 ? Brushes.Red : Brushes.Green;
        }

        private void ExpensesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedExpense = ExpensesDataGrid.SelectedItem as Expense;
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense != null)
            {
                EditItemNameTextBox.Text = _selectedExpense.ItemName;
                EditAmountSpentTextBox.Text = _selectedExpense.AmountSpent.ToString("N0");
                EditExpenseDatePicker.SelectedDate = DateTime.Parse(_selectedExpense.Date);

                foreach (ComboBoxItem item in EditItemTypeComboBox.Items)
                {
                    if (item.Content.ToString() == _selectedExpense.ItemType)
                    {
                        EditItemTypeComboBox.SelectedItem = item;
                        break;
                    }
                }

                EditSection.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Please select an expense to edit.", "Edit Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense != null)
            {
                // Retrieve the updated values from the UI elements
                string itemName = EditItemNameTextBox.Text;
                string itemType = (EditItemTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                decimal amountSpent = decimal.TryParse(EditAmountSpentTextBox.Text, out decimal amount) ? amount : _selectedExpense.AmountSpent;
                string currency = ExpenseDataStore.CurrencySymbol;  // Or use a selected value for currency
                string date = EditExpenseDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? _selectedExpense.Date;

                // Call UpdateExpense with the required arguments
                ExpenseDataStore.UpdateExpense(_selectedExpense.ExpenseID, itemName, itemType, amountSpent, currency, date);

                // Optionally, you can also refresh the DataGrid or UI after updating
                ExpensesDataGrid.Items.Refresh(); // Refresh the grid to reflect changes

                // Hide the edit section
                EditSection.Visibility = Visibility.Collapsed;

                MessageBox.Show("Expense updated successfully.", "Edit Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSection.Visibility = Visibility.Collapsed;
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete this expense?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ExpenseDataStore.DeleteExpense(_selectedExpense.ExpenseID);

                    _selectedExpense = null; // Clear selection after deletion
                    UpdateRemainingIncomeDisplay();

                    MessageBox.Show("Expense deleted successfully.", "Delete Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select an expense to delete.", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
