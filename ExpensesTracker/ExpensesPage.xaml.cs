using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class ExpensesPage : Page
    {
        private bool isUpdatingTextBox = false;

        public ExpensesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load income and currency details for the current user
            IncomeTextBox.Text = FormatCurrency(ExpenseDataStore.Income);
            CurrencyComboBox.SelectedIndex = ExpenseDataStore.CurrencySymbol == "$" ? 0 : 1;
            SyncExpenseCurrencyWithIncome();
        }

        private void IncomeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTextBox) return;

            isUpdatingTextBox = true;
            try
            {
                if (decimal.TryParse(IncomeTextBox.Text.Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal income))
                {
                    string symbol = GetCurrencySymbol();
                    IncomeTextBox.Text = $"{symbol}{income:N0}";
                    IncomeTextBox.CaretIndex = IncomeTextBox.Text.Length;

                    // Update the user's income in the ExpenseDataStore and database
                    ExpenseDataStore.UpdateIncome(income);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating income: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isUpdatingTextBox = false;
            }
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update currency symbol and refresh inputs
            ExpenseDataStore.CurrencySymbol = GetCurrencySymbol();
            IncomeTextBox_TextChanged(IncomeTextBox, null);
            SyncExpenseCurrencyWithIncome();
        }

        private void SyncExpenseCurrencyWithIncome()
        {
            // Ensure AmountSpentTextBox matches the current currency
            ExpenseCurrencyComboBox.SelectedIndex = CurrencyComboBox.SelectedIndex;
            AmountSpentTextBox_TextChanged(AmountSpentTextBox, null);
        }

        private void AmountSpentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTextBox) return;

            isUpdatingTextBox = true;
            try
            {
                if (decimal.TryParse(AmountSpentTextBox.Text.Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal amountSpent))
                {
                    string symbol = GetSelectedCurrencySymbol();
                    AmountSpentTextBox.Text = $"{symbol}{amountSpent:N0}";
                    AmountSpentTextBox.CaretIndex = AmountSpentTextBox.Text.Length;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error formatting amount: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isUpdatingTextBox = false;
            }
        }

        private void ExpenseCurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reformat the amount spent when currency changes
            AmountSpentTextBox_TextChanged(AmountSpentTextBox, null);
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse Amount Spent
                if (!decimal.TryParse(AmountSpentTextBox.Text.Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal amountSpent))
                {
                    MessageBox.Show("Amount must be a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Collect Inputs
                string itemName = ItemNameTextBox.Text.Trim();
                string itemType = (ItemTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string expenseCurrency = GetSelectedCurrencySymbol();
                string incomeCurrency = ExpenseDataStore.CurrencySymbol;
                string date = ExpenseDatePicker.SelectedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                              ?? DateTime.Now.ToString("yyyy-MM-dd");

                // Validate Inputs
                if (string.IsNullOrWhiteSpace(itemName) || string.IsNullOrWhiteSpace(itemType))
                {
                    MessageBox.Show("Please fill all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Convert currency if necessary
                if (expenseCurrency != incomeCurrency)
                {
                    amountSpent = ConvertCurrency(amountSpent, expenseCurrency, incomeCurrency);
                }

                // Add expense using ExpenseDataStore
                ExpenseDataStore.AddExpense(itemName, itemType, amountSpent, incomeCurrency, date);

                // Update UI
                MessageBox.Show("Expense added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearInputFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            const decimal USD_TO_IDR_RATE = 14000; // Conversion rate: $1 = Rp14,000

            if (fromCurrency == "$" && toCurrency == "Rp")
            {
                return amount * USD_TO_IDR_RATE;
            }
            else if (fromCurrency == "Rp" && toCurrency == "$")
            {
                return amount / USD_TO_IDR_RATE;
            }

            return amount; // No conversion needed if currencies are the same
        }

        private void ClearInputFields()
        {
            ItemNameTextBox.Clear();
            ItemTypeComboBox.SelectedIndex = -1;
            AmountSpentTextBox.Clear();
            ExpenseDatePicker.SelectedDate = null;
            SyncExpenseCurrencyWithIncome();
        }

        private string GetCurrencySymbol()
        {
            return (CurrencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() == "Dollar" ? "$" : "Rp";
        }

        private string GetSelectedCurrencySymbol()
        {
            return (ExpenseCurrencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() == "Dollar" ? "$" : "Rp";
        }

        private string FormatCurrency(decimal amount)
        {
            string symbol = GetCurrencySymbol();
            return $"{symbol}{amount:N0}";
        }
    }
}
