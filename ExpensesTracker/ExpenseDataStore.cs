using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExpensesTracker
{
    public static class ExpenseDataStore
    {
        public static ObservableCollection<Expense> Expenses { get; } = new ObservableCollection<Expense>();

        // User-specific data
        public static decimal Income { get; set; } = 0;
        public static int UserID { get; set; }
        public static string UserEmail { get; set; }
        public static int UserAge { get; set; }
        public static string UserGender { get; set; }
        public static string CurrencySymbol { get; set; } = "$";

        // Other features
        public static string LastGeneratedTips { get; set; } = string.Empty;
        public static string GoalName { get; set; } = string.Empty;
        public static DateTime GoalDeadline { get; set; } = DateTime.MinValue;
        public static decimal FinancialGoal { get; set; } = 0m;
        public static int ApiLimit { get; set; } = 100;
        public static int ApiCallCount { get; set; } = 0;

        // Calculate remaining income
        public static decimal RemainingIncome => Income - Expenses.Sum(e => e.AmountSpent);

        /// <summary>
        /// Load expenses from the database for the logged-in user.
        /// </summary>
        public static void LoadExpenses()
        {
            Expenses.Clear();
            var expensesFromDb = DatabaseHelper.GetExpenses(UserID); // Ensure UserID is passed here

            foreach (var expense in expensesFromDb)
            {
                Expenses.Add(expense);
            }
            Console.WriteLine($"Loaded {Expenses.Count} expenses for UserID: {UserID}");
        }



        /// <summary>
        /// Add a new expense and save it to the database.
        /// </summary>
        public static void AddExpense(string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            // Convert currency if it differs from user's income currency
            if (currency != CurrencySymbol)
            {
                amountSpent = ConvertCurrency(amountSpent, currency, CurrencySymbol);
                currency = CurrencySymbol;
            }

            bool success = DatabaseHelper.AddExpense(UserID, itemName, itemType, amountSpent, currency, date);

            if (success)
            {
                Expenses.Add(new Expense
                {
                    ExpenseID = Expenses.Any() ? Expenses.Max(e => e.ExpenseID) + 1 : 1,
                    UserID = UserID,
                    ItemName = itemName,
                    ItemType = itemType,
                    AmountSpent = amountSpent,
                    Currency = currency,
                    Date = date,
                    AmountSpentDisplay = $"{CurrencySymbol}{amountSpent:N0}"
                });

                Console.WriteLine("Expense added successfully.");
            }
        }

        /// <summary>
        /// Update an existing expense in the database.
        /// </summary>
        public static void UpdateExpense(int expenseId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            // Convert currency if it differs from user's income currency
            if (currency != CurrencySymbol)
            {
                amountSpent = ConvertCurrency(amountSpent, currency, CurrencySymbol);
                currency = CurrencySymbol;
            }

            DatabaseHelper.UpdateExpense(expenseId, itemName, itemType, amountSpent, currency, date);

            var expenseToUpdate = Expenses.FirstOrDefault(e => e.ExpenseID == expenseId);
            if (expenseToUpdate != null)
            {
                expenseToUpdate.ItemName = itemName;
                expenseToUpdate.ItemType = itemType;
                expenseToUpdate.AmountSpent = amountSpent;
                expenseToUpdate.Currency = currency;
                expenseToUpdate.Date = date;
                expenseToUpdate.AmountSpentDisplay = $"{CurrencySymbol}{amountSpent:N0}";

                Console.WriteLine($"Expense with ID {expenseId} updated successfully.");
            }
        }

        /// <summary>
        /// Delete an expense from the database.
        /// </summary>
        public static void DeleteExpense(int expenseId)
        {
            DatabaseHelper.DeleteExpense(expenseId);

            var expenseToRemove = Expenses.FirstOrDefault(e => e.ExpenseID == expenseId);
            if (expenseToRemove != null)
            {
                Expenses.Remove(expenseToRemove);
                Console.WriteLine($"Expense with ID {expenseId} deleted successfully.");
            }
        }

        /// <summary>
        /// Update the user's income and save it to the database.
        /// </summary>
        public static void UpdateIncome(decimal newIncome)
        {
            Income = newIncome;
            DatabaseHelper.UpdateIncome(UserID, newIncome);

            Console.WriteLine($"Income updated for UserID: {UserID}");
        }

        /// <summary>
        /// Convert currency amounts based on predefined exchange rates.
        /// </summary>
        private static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
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

        /// <summary>
        /// Converts an expense's amount to the user's currency for display.
        /// </summary>
        private static decimal ConvertCurrencyForDisplay(decimal amount, string fromCurrency)
        {
            return ConvertCurrency(amount, fromCurrency, CurrencySymbol);
        }
    }

    public class Expense
    {
        public int ExpenseID { get; set; }
        public int UserID { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public decimal AmountSpent { get; set; }
        public string Currency { get; set; }
        public string Date { get; set; }

        // New property for formatted display of Amount Spent
        public string AmountSpentDisplay { get; set; }
    }
}
