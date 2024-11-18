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
            CurrencyComboBox.SelectedIndex = ExpenseDataStore.CurrencySymbol == "$" ? 0 : 1; // Load last-used currency
            IncomeTextBox.Text = FormatIncomeText(ExpenseDataStore.Income); // Load last-used income
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load persisted income and currency when the page loads
            IncomeTextBox.Text = FormatIncomeText(ExpenseDataStore.Income);
            CurrencyComboBox.SelectedIndex = ExpenseDataStore.CurrencySymbol == "$" ? 0 : 1;
        }

        private void IncomeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTextBox) return;

            isUpdatingTextBox = true;
            TextBox textBox = sender as TextBox;

            if (decimal.TryParse(textBox.Text.Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal income))
            {
                // Display the currency symbol based on selected income currency
                string symbol = (CurrencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() == "Dollar" ? "$" : "Rp";
                textBox.Text = $"{symbol}{income:N0}";
                textBox.CaretIndex = textBox.Text.Length;
                ExpenseDataStore.Income = income; // Persist income
            }
            isUpdatingTextBox = false;
        }

        private void CurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrencyComboBox.SelectedItem is ComboBoxItem selectedCurrency)
            {
                ExpenseDataStore.CurrencySymbol = selectedCurrency.Content.ToString() == "Dollar" ? "$" : "Rp";
                ExpenseCurrencyComboBox.SelectedIndex = CurrencyComboBox.SelectedIndex;

                IncomeTextBox_TextChanged(IncomeTextBox, null);
                AmountSpentTextBox_TextChanged(AmountSpentTextBox, null);
            }
        }

        private void AmountSpentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTextBox) return;

            isUpdatingTextBox = true;
            TextBox textBox = sender as TextBox;

            if (decimal.TryParse(textBox.Text.Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal amountSpent))
            {
                string symbol = (ExpenseCurrencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() == "Dollar" ? "$" : "Rp";
                textBox.Text = $"{symbol}{amountSpent:N0}";
                textBox.CaretIndex = textBox.Text.Length;
            }
            isUpdatingTextBox = false;
        }

        private void ExpenseCurrencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AmountSpentTextBox_TextChanged(AmountSpentTextBox, null);
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountSpentTextBox.Text.Replace(",", "").Replace("Rp", "").Replace("$", ""), out decimal amountSpent))
            {
                string itemName = ItemNameTextBox.Text;
                string itemType = (ItemTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Other";
                string currency = (ExpenseCurrencyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string date = ExpenseDatePicker.SelectedDate?.ToString("d", CultureInfo.InvariantCulture) ?? DateTime.Now.ToString("d");

                // Add the expense to the data store
                ExpenseDataStore.Expenses.Add(new Expense
                {
                    ItemName = itemName,
                    ItemType = itemType,
                    AmountSpent = amountSpent,
                    Currency = currency,
                    Date = date
                });

                MessageBox.Show("Expense added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear input fields after adding
                ClearInputFields();
            }
            else
            {
                MessageBox.Show("Amount must be a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearInputFields()
        {
            ItemNameTextBox.Clear();
            ItemTypeComboBox.SelectedIndex = -1;
            AmountSpentTextBox.Clear();
            ExpenseDatePicker.SelectedDate = null;
            ExpenseCurrencyComboBox.SelectedIndex = CurrencyComboBox.SelectedIndex;
        }

        private string FormatIncomeText(decimal income)
        {
            string symbol = ExpenseDataStore.CurrencySymbol;
            return $"{symbol}{income:N0}";
        }
    }
}
