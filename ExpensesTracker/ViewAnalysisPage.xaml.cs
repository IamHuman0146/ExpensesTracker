using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class ViewAnalysisPage : Page
    {
        private Expense _selectedExpense;

        public ViewAnalysisPage()
        {
            InitializeComponent();

            InitializeMonthFilter();
            LoadExpenses();
            UpdateIncomeAndRemainingDisplay();

            // Automatically refresh when the Expenses collection changes
            ExpenseDataStore.Expenses.CollectionChanged += (s, e) =>
            {
                RefreshDataGrid();
                UpdateIncomeAndRemainingDisplay();
            };

            ToggleEditDeleteButtons(false);
        }

        private void InitializeMonthFilter()
        {
            int currentMonth = DateTime.Now.Month;
            MonthFilterComboBox.SelectedIndex = currentMonth; // Set the ComboBox default to the current month
        }

        private void LoadExpenses()
        {
            // Reload expenses from data store
            ExpenseDataStore.LoadExpenses();

            // Format the amount display for all expenses
            foreach (var expense in ExpenseDataStore.Expenses)
            {
                expense.AmountSpentDisplay = $"{ExpenseDataStore.CurrencySymbol}{expense.AmountSpent:N0}";
            }

            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            int selectedMonth = GetSelectedMonth();

            if (selectedMonth == -1) // All data
            {
                ExpensesDataGrid.ItemsSource = ExpenseDataStore.Expenses;
            }
            else // Filter by month
            {
                var filteredExpenses = ExpenseDataStore.Expenses
                    .Where(expense => DateTime.Parse(expense.Date).Month == selectedMonth)
                    .ToList();

                ExpensesDataGrid.ItemsSource = filteredExpenses;
            }

            ExpensesDataGrid.Items.Refresh();
        }

        private int GetSelectedMonth()
        {
            if (MonthFilterComboBox.SelectedItem == null) return -1;
            return Convert.ToInt32((MonthFilterComboBox.SelectedItem as ComboBoxItem)?.Tag);
        }

        private void UpdateIncomeAndRemainingDisplay()
        {
            int selectedMonth = GetSelectedMonth();
            int currentMonth = DateTime.Now.Month;
            string monthName = selectedMonth == -1
                ? DateTime.Now.ToString("MMMM")
                : new DateTime(DateTime.Now.Year, selectedMonth, 1).ToString("MMMM");

            IncomeLabel.Text = $"Current Income ({monthName})";
            RemainingIncomeLabel.Text = $"Remaining Income ({monthName})";

            decimal currentIncome = ExpenseDataStore.Income;

            decimal remainingIncome;
            if (selectedMonth == -1)
            {
                remainingIncome = currentIncome - ExpenseDataStore.Expenses
                    .Where(e => DateTime.Parse(e.Date).Month == currentMonth)
                    .Sum(e => e.AmountSpent);
            }
            else
            {
                remainingIncome = currentIncome - ExpenseDataStore.Expenses
                    .Where(e => DateTime.Parse(e.Date).Month == selectedMonth)
                    .Sum(e => e.AmountSpent);
            }

            IncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{currentIncome:N0}";
            RemainingIncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{remainingIncome:N0}";
        }

        private void MonthFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshDataGrid();
            UpdateIncomeAndRemainingDisplay();
        }

        private void ExpensesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedExpense = ExpensesDataGrid.SelectedItem as Expense;
            ToggleEditDeleteButtons(_selectedExpense != null);
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

            EditItemNameTextBox.Text = _selectedExpense.ItemName;
            EditAmountSpentTextBox.Text = _selectedExpense.AmountSpent.ToString();
            EditExpenseDatePicker.SelectedDate = DateTime.Parse(_selectedExpense.Date);

            foreach (ComboBoxItem item in EditItemTypeComboBox.Items)
            {
                if (item.Content.ToString() == _selectedExpense.ItemType)
                {
                    EditItemTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            EditOverlay.Visibility = Visibility.Visible;
            MainContent.IsEnabled = false;
        }

        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

            // Validate the input fields
            string itemName = EditItemNameTextBox.Text;
            string itemType = (EditItemTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool isAmountValid = decimal.TryParse(EditAmountSpentTextBox.Text, out decimal amountSpent);
            string date = EditExpenseDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(itemName))
            {
                MessageBox.Show("Item Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(itemType))
            {
                MessageBox.Show("Please select an item type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!isAmountValid || amountSpent <= 0)
            {
                MessageBox.Show("Amount Spent must be a valid positive number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(date))
            {
                MessageBox.Show("Please select a valid date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update expense details
            ExpenseDataStore.UpdateExpense(_selectedExpense.ExpenseID, itemName, itemType, amountSpent, ExpenseDataStore.CurrencySymbol, date);

            LoadExpenses();
            EditOverlay.Visibility = Visibility.Collapsed;
            MainContent.IsEnabled = true;
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            EditOverlay.Visibility = Visibility.Collapsed;
            MainContent.IsEnabled = true;
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this expense?",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ExpenseDataStore.DeleteExpense(_selectedExpense.ExpenseID);
                LoadExpenses();
                MessageBox.Show("Expense deleted successfully.", "Delete Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ToggleEditDeleteButtons(bool isEnabled)
        {
            EditButton.IsEnabled = isEnabled;
            DeleteButton.IsEnabled = isEnabled;
        }
    }
}
