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
        public static decimal CalculatedBudget { get; set; } = 0; // Budget calculated in SetGoals
        public static int UserID { get; set; }
        public static string UserEmail { get; set; }
        public static int UserAge { get; set; }
        public static string UserGender { get; set; }
        public static string CurrencySymbol { get; set; } = "$";

        public static string LastGeneratedTips { get; set; } = string.Empty;
        public static string GoalName { get; set; } = string.Empty;
        public static DateTime GoalDeadline { get; set; } = DateTime.MinValue;
        public static decimal FinancialGoal { get; set; } = 0m;
        public static int ApiLimit { get; set; } = 100;
        public static int ApiCallCount { get; set; } = 0;

        // Calculate remaining income
        public static decimal RemainingIncome => Income - Expenses.Sum(e => e.AmountSpent);

        // Event to notify other pages of budget updates
        public static event Action BudgetUpdated;

        public static void UpdateCalculatedBudget()
        {
            CalculatedBudget = CalculateTotalBudget();

            // Notify other parts of the app about the budget update
            NotifyBudgetUpdated();
        }

        public static decimal CalculateTotalBudget()
        {
            decimal pastMonthsIncome = CalculatePastMonthsIncome();
            decimal remainingIncome = Income - Expenses.Sum(e => e.AmountSpent);
            return pastMonthsIncome + remainingIncome;
        }

        private static decimal CalculatePastMonthsIncome()
        {
            DateTime today = DateTime.Now;
            DateTime firstExpenseDate = Expenses
                .Select(e => DateTime.Parse(e.Date))
                .OrderBy(d => d)
                .FirstOrDefault();

            if (firstExpenseDate == default)
                return 0;

            int monthsPassed = (today.Year - firstExpenseDate.Year) * 12 + today.Month - firstExpenseDate.Month;
            return Math.Max(0, monthsPassed * Income);
        }

        public static void LoadExpenses()
        {
            Expenses.Clear();
            var expensesFromDb = DatabaseHelper.GetExpenses(UserID);

            foreach (var expense in expensesFromDb)
            {
                Expenses.Add(expense);
            }
            Console.WriteLine($"Loaded {Expenses.Count} expenses for UserID: {UserID}");
        }

        public static void AddExpense(string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
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
            }
        }

        public static void UpdateExpense(int expenseId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
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
            }
        }

        public static void DeleteExpense(int expenseId)
        {
            DatabaseHelper.DeleteExpense(expenseId);

            var expenseToRemove = Expenses.FirstOrDefault(e => e.ExpenseID == expenseId);
            if (expenseToRemove != null)
            {
                Expenses.Remove(expenseToRemove);
            }
        }

        public static void UpdateIncome(decimal newIncome)
        {
            Income = newIncome;
            DatabaseHelper.UpdateIncome(UserID, newIncome);

            // Notify about budget update
            NotifyBudgetUpdated();
        }

        // Updated method to handle goal outcomes (Spent/Kept) with persistence
        public static void ApplyGoalOutcome(string goalName, decimal amount, string outcome)
        {
            if (outcome == "Spent")
            {
                CalculatedBudget -= amount;
            }
            // If "Kept", no change to the budget

            BudgetHelper.SaveBudget(UserID, CalculatedBudget); // Persist the updated budget
            NotifyBudgetUpdated();
        }

        private static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            const decimal USD_TO_IDR_RATE = 14000;

            if (fromCurrency == "$" && toCurrency == "Rp")
                return amount * USD_TO_IDR_RATE;
            else if (fromCurrency == "Rp" && toCurrency == "$")
                return amount / USD_TO_IDR_RATE;

            return amount;
        }

        // Notify subscribers when the budget is updated
        public static void NotifyBudgetUpdated()
        {
            BudgetUpdated?.Invoke();
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
        public string AmountSpentDisplay { get; set; }
    }
}
