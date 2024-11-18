using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.SQLite;

namespace ExpensesTracker
{
    public static class ExpenseDataStore
    {
        public static ObservableCollection<Expense> Expenses { get; } = new ObservableCollection<Expense>();
        public static decimal Income { get; set; } = 0;
        public static string CurrencySymbol { get; set; } = "$"; // Default to Dollar
        public static int UserID { get; set; } // To track logged-in user
        public static string UserEmail { get; set; }
        public static int UserAge { get; set; }
        public static string UserGender { get; set; }
        public static string LastGeneratedTips { get; set; } = "";
        public static int ApiCallCount { get; set; }
        public static int ApiLimit { get; set; } = 100;

        public static string GoalName { get; set; } = string.Empty;
        public static DateTime GoalDeadline { get; set; } = DateTime.MinValue;
        public static decimal FinancialGoal { get; set; } = 0m;

        public static decimal RemainingIncome
        {
            get
            {
                decimal totalExpenses = Expenses.Sum(expense => expense.AmountSpent);
                return Income - totalExpenses;
            }
        }

        public static void LoadExpenses()
        {
            Expenses.Clear(); // Clear existing data
            var expensesFromDb = DatabaseHelper.GetExpenses(UserID); // Fetch expenses for the logged-in user
            foreach (var expense in expensesFromDb)
            {
                Expenses.Add(expense);
            }
        }

        public static void AddExpense(string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            bool success = DatabaseHelper.AddExpense(UserID, itemName, itemType, amountSpent, currency, date);
            if (success)
            {
                Expenses.Add(new Expense
                {
                    UserID = UserID,
                    ItemName = itemName,
                    ItemType = itemType,
                    AmountSpent = amountSpent,
                    Currency = currency,
                    Date = date
                });
            }
        }

        public static void UpdateExpense(int expenseId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
            {
                using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString())) // Use the public method
                {
                    connection.Open();
                    var command = new SQLiteCommand(@"
                        UPDATE Expenses
                        SET ItemName = @ItemName, ItemType = @ItemType, AmountSpent = @AmountSpent, Currency = @Currency, Date = @Date
                        WHERE ExpenseID = @ExpenseID", connection);

                    // Add parameters to the SQL query
                    command.Parameters.AddWithValue("@ExpenseID", expenseId);
                    command.Parameters.AddWithValue("@ItemName", itemName);
                    command.Parameters.AddWithValue("@ItemType", itemType);
                    command.Parameters.AddWithValue("@AmountSpent", amountSpent);
                    command.Parameters.AddWithValue("@Currency", currency);
                    command.Parameters.AddWithValue("@Date", date);

                    // Execute the command
                    command.ExecuteNonQuery();
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Error updating expense: {ex.Message}");
            }
        }

        public static void DeleteExpense(int expenseId)
        {
            bool success = DatabaseHelper.DeleteExpense(expenseId, UserID);
            if (success)
            {
                var expense = Expenses.FirstOrDefault(e => e.ExpenseID == expenseId);
                if (expense != null)
                {
                    Expenses.Remove(expense);
                }
            }
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
    }
}
