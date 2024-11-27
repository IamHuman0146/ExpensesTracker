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
<<<<<<< HEAD
=======

            // Load expenses for the logged-in user
            LoadExpenses();
            ExpensesDataGrid.ItemsSource = ExpenseDataStore.Expenses;
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d

            InitializeMonthFilter();
            LoadExpenses();
            UpdateIncomeAndRemainingDisplay();

            // Automatically refresh when the Expenses collection changes
            ExpenseDataStore.Expenses.CollectionChanged += (s, e) =>
            {
<<<<<<< HEAD
                RefreshDataGrid();
                UpdateIncomeAndRemainingDisplay();
            };

            ToggleEditDeleteButtons(false);
=======
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
                // Fetch expenses for the logged-in user
                ExpenseDataStore.LoadExpenses();

                foreach (var expense in ExpenseDataStore.Expenses)
                {
                    // Format `AmountSpent` for display
                    expense.AmountSpentDisplay = $"{ExpenseDataStore.CurrencySymbol}{expense.AmountSpent:N0}";
                }

                ExpensesDataGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load expenses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
        }

        private void InitializeMonthFilter()
        {
<<<<<<< HEAD
            int currentMonth = DateTime.Now.Month;
            MonthFilterComboBox.SelectedIndex = currentMonth; // Set the ComboBox default to the current month
=======
            try
            {
                IncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{ExpenseDataStore.Income:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying income: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
        }

        private void LoadExpenses()
        {
<<<<<<< HEAD
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
=======
            try
            {
                decimal remainingIncome = ExpenseDataStore.RemainingIncome;
                RemainingIncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{remainingIncome:N0}";

                // Change text color based on value
                RemainingIncomeTextBlock.Foreground = remainingIncome < 0 ? Brushes.Red : Brushes.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying remaining income: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
        }

        private void ExpensesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected expense from the DataGrid
            _selectedExpense = ExpensesDataGrid.SelectedItem as Expense;
<<<<<<< HEAD
=======

            // Enable or disable the Edit and Delete buttons based on selection
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
            ToggleEditDeleteButtons(_selectedExpense != null);
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD
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
=======
            if (_selectedExpense != null)
            {
                try
                {
                    // Populate the edit fields with the selected expense's data
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

                    // Show the edit section
                    EditSectionScrollViewer.Visibility = Visibility.Visible;

                    // Disable Edit/Delete buttons while editing
                    ToggleEditDeleteButtons(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to prepare the edit form: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
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
<<<<<<< HEAD
                MessageBox.Show("Item Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
=======
                try
                {
                    // Retrieve the updated values from the UI elements
                    string itemName = EditItemNameTextBox.Text;
                    string itemType = (EditItemTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    decimal amountSpent = decimal.TryParse(EditAmountSpentTextBox.Text, out decimal amount) ? amount : _selectedExpense.AmountSpent;
                    string currency = ExpenseDataStore.CurrencySymbol;
                    string date = EditExpenseDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? _selectedExpense.Date;

                    // Update the expense in the database
                    ExpenseDataStore.UpdateExpense(_selectedExpense.ExpenseID, itemName, itemType, amountSpent, currency, date);

                    // Refresh the DataGrid
                    LoadExpenses();

                    // Hide the edit section and re-enable Edit/Delete buttons
                    EditSectionScrollViewer.Visibility = Visibility.Collapsed;
                    ToggleEditDeleteButtons(true);

                    MessageBox.Show("Expense updated successfully.", "Edit Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save the changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
            }

<<<<<<< HEAD
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
=======
        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide the edit section without saving changes
                EditSectionScrollViewer.Visibility = Visibility.Collapsed;

                // Re-enable Edit/Delete buttons
                ToggleEditDeleteButtons(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to cancel editing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense == null) return;

            var result = MessageBox.Show("Are you sure you want to delete this expense?",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
<<<<<<< HEAD
                ExpenseDataStore.DeleteExpense(_selectedExpense.ExpenseID);
                LoadExpenses();
                MessageBox.Show("Expense deleted successfully.", "Delete Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
=======
                var result = MessageBox.Show("Are you sure you want to delete this expense?",
                                             "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Delete the selected expense from the database
                        ExpenseDataStore.DeleteExpense(_selectedExpense.ExpenseID);

                        // Clear the selection and reload the DataGrid
                        _selectedExpense = null;
                        LoadExpenses();

                        MessageBox.Show("Expense deleted successfully.", "Delete Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete the expense: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ToggleEditDeleteButtons(bool isVisible)
        {
            EditButton.IsEnabled = isVisible;
            DeleteButton.IsEnabled = isVisible;
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
        }

        private void ToggleEditDeleteButtons(bool isEnabled)
        {
            EditButton.IsEnabled = isEnabled;
            DeleteButton.IsEnabled = isEnabled;
        }
    }
}
