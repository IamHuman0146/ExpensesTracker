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
        }

        private void UpdateIncomeDisplay()
        {
            try
            {
                IncomeTextBlock.Text = $"{ExpenseDataStore.CurrencySymbol}{ExpenseDataStore.Income:N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying income: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRemainingIncomeDisplay()
        {
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
        }

        private void ExpensesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected expense from the DataGrid
            _selectedExpense = ExpensesDataGrid.SelectedItem as Expense;

            // Enable or disable the Edit and Delete buttons based on selection
            ToggleEditDeleteButtons(_selectedExpense != null);
        }

        private void EditExpense_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense != null)
            {
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
            }
        }

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
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedExpense != null)
            {
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
        }
    }
}
