using System;
using System.Globalization;
using System.Linq;
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

            // Load expenses for the logged-in user
            LoadExpenses();
            ExpensesDataGrid.ItemsSource = ExpenseDataStore.Expenses;

            UpdateIncomeDisplay();
            UpdateRemainingIncomeDisplay();

            // Refresh income and remaining income when expenses change
            ExpenseDataStore.Expenses.CollectionChanged += (s, e) =>
            {
                UpdateRemainingIncomeDisplay();
                ExpensesDataGrid.Items.Refresh();
            };

            // Initially disable the Edit and Delete buttons
            ToggleEditDeleteButtons(false);
        }

        private void LoadExpenses()
        {
            try
            {
                ExpenseDataStore.LoadExpenses();

                foreach (var expense in ExpenseDataStore.Expenses)
                {
                    expense.AmountSpentDisplay = $"{ExpenseDataStore.CurrencySymbol}{expense.AmountSpent:N0}";
                }

                ExpensesDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load expenses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateIncomeDisplay()
        {
            IncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{ExpenseDataStore.Income:N0}";
        }

        private void UpdateRemainingIncomeDisplay()
        {
            decimal remainingIncome = ExpenseDataStore.RemainingIncome;
            RemainingIncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{remainingIncome:N0}";
            RemainingIncomeTextBlock.Foreground = remainingIncome < 0 ? Brushes.Red : Brushes.Green;
        }

        private void ExpensesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedExpense = ExpensesDataGrid.SelectedItem as Expense;
            ToggleEditDeleteButtons(_selectedExpense != null);
        }

        private void FilterByMonth_Click(object sender, RoutedEventArgs e)
        {
            if (MonthFilterComboBox.SelectedItem == null) return;

            var selectedMonth = Convert.ToInt32((MonthFilterComboBox.SelectedItem as ComboBoxItem)?.Tag);
            var filteredExpenses = ExpenseDataStore.Expenses
                .Where(expense =>
                {
                    var expenseDate = DateTime.Parse(expense.Date);
                    return expenseDate.Month == selectedMonth;
                })
                .ToList();

            ExpensesDataGrid.ItemsSource = filteredExpenses;
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

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

            EditSectionScrollViewer.Visibility = Visibility.Visible;
            ToggleEditDeleteButtons(false);
        }

        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

            string itemName = EditItemNameTextBox.Text;
            string itemType = (EditItemTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            decimal amountSpent = decimal.TryParse(EditAmountSpentTextBox.Text, out decimal amount) ? amount : _selectedExpense.AmountSpent;
            string date = EditExpenseDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? _selectedExpense.Date;

            ExpenseDataStore.UpdateExpense(_selectedExpense.ExpenseID, itemName, itemType, amountSpent, ExpenseDataStore.CurrencySymbol, date);
            LoadExpenses();

            EditSectionScrollViewer.Visibility = Visibility.Collapsed;
            ToggleEditDeleteButtons(true);

            MessageBox.Show("Expense updated successfully.", "Edit Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            EditSectionScrollViewer.Visibility = Visibility.Collapsed;
            ToggleEditDeleteButtons(true);
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this expense?",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ExpenseDataStore.DeleteExpense(_selectedExpense.ExpenseID);
                _selectedExpense = null;
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
