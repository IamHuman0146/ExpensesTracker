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
        public static decimal CalculatedBudget { get; set; } = 0; // Budget calculated dynamically
        public static decimal InitialBudget { get; set; } = 0;   // Tracks the starting budget before deductions
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

        // Remaining income after all expenses
        public static decimal RemainingIncome => Income - Expenses.Sum(e => e.AmountSpent);

        // Event to notify other pages of budget updates
        public static event Action BudgetUpdated;

        /// <summary>
        /// Recalculates the calculated budget and persists it.
        /// </summary>
        public static void UpdateCalculatedBudget()
        {
            try
            {
                // Ensure Expenses and other related data are up-to-date
                LoadExpenses();

                // Perform the calculation
                CalculatedBudget = CalculateTotalBudget();

                // Save the updated budget using BudgetHelper
                BudgetHelper.SaveBudget(UserID, CalculatedBudget);

                // Notify subscribers about the updated budget
                NotifyBudgetUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating budget: {ex.Message}");
            }
        }

        public static void InitializeDataStore(int userId)
        {
            try
            {
                UserID = userId;

                // Load expenses from the database
                LoadExpenses();

                // Load the budget from BudgetHelper
                CalculatedBudget = BudgetHelper.LoadBudget(userId);

                // If no budget exists, calculate it for the first time
                if (CalculatedBudget == 0)
                {
                    UpdateCalculatedBudget();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ExpenseDataStore: {ex.Message}");
            }
        }




        /// <summary>
        /// Calculates the total budget, including past months' income and remaining income.
        /// </summary>
        /// <returns>Updated total budget.</returns>
        public static decimal CalculateTotalBudget()
        {
            try
            {
                decimal pastMonthsIncome = CalculatePastMonthsIncome();
                decimal remainingIncome = RemainingIncome;

                // Update the initial budget if it's not already set
                if (InitialBudget == 0)
                {
                    InitialBudget = pastMonthsIncome + remainingIncome;
                }

                return pastMonthsIncome + remainingIncome;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating total budget: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Calculates past months' income based on the user's income and the date of the first recorded expense.
        /// </summary>
        /// <returns>Total past months' income.</returns>
        private static decimal CalculatePastMonthsIncome()
        {
            try
            {
                DateTime today = DateTime.Now;

                // Determine the date of the first recorded expense
                DateTime firstExpenseDate = Expenses
                    .Select(e => DateTime.Parse(e.Date))
                    .OrderBy(d => d)
                    .FirstOrDefault();

                if (firstExpenseDate == default)
                    return 0;

                // Calculate the number of months passed since the first expense
                int monthsPassed = (today.Year - firstExpenseDate.Year) * 12 + today.Month - firstExpenseDate.Month;

                // Calculate the past months' income as remaining income per month
                return Math.Max(0, monthsPassed * Income); // Ensure it's only adding the income for each past month
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating past months' income: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Loads expenses from the database into memory.
        /// </summary>
        public static void LoadExpenses()
        {
            try
            {
                Expenses.Clear();
                var expensesFromDb = DatabaseHelper.GetExpenses(UserID);

                foreach (var expense in expensesFromDb)
                {
                    Expenses.Add(expense);
                }

                Console.WriteLine($"Loaded {Expenses.Count} expenses for UserID: {UserID}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading expenses: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a new expense and updates the budget.
        /// </summary>
        public static void AddExpense(string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
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

                    // Recalculate the budget
                    UpdateCalculatedBudget();

                    // Notify all subscribers that the budget has been updated
                    NotifyBudgetUpdated();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding expense: {ex.Message}");
            }
        }


        /// <summary>
        /// Updates an existing expense.
        /// </summary>
        public static void UpdateExpense(int expenseId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
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

                // Update calculated budget
                UpdateCalculatedBudget();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating expense: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes an expense from the database and memory.
        /// </summary>
        public static void DeleteExpense(int expenseId)
        {
            try
            {
                DatabaseHelper.DeleteExpense(expenseId);

                var expenseToRemove = Expenses.FirstOrDefault(e => e.ExpenseID == expenseId);
                if (expenseToRemove != null)
                {
                    Expenses.Remove(expenseToRemove);

                    // Update calculated budget
                    UpdateCalculatedBudget();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting expense: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the user's income and recalculates the budget.
        /// </summary>
        public static void UpdateIncome(decimal newIncome)
        {
            try
            {
                Income = newIncome;
                DatabaseHelper.UpdateIncome(UserID, newIncome);

                // Recalculate budget
                UpdateCalculatedBudget();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating income: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies a goal's outcome (e.g., spent, saved) and updates the budget.
        /// </summary>
        public static void ApplyGoalOutcome(string goalName, decimal amount, string outcome)
        {
            try
            {
                if (outcome == "Spent")
                {
                    CalculatedBudget -= amount; // Deduct the goal amount from the budget
                }

                BudgetHelper.SaveBudget(UserID, CalculatedBudget);
                NotifyBudgetUpdated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying goal outcome: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts currency amounts between two specified currencies.
        /// </summary>
        private static decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            try
            {
                const decimal USD_TO_IDR_RATE = 14000;

                if (fromCurrency == "$" && toCurrency == "Rp")
                    return amount * USD_TO_IDR_RATE;
                else if (fromCurrency == "Rp" && toCurrency == "$")
                    return amount / USD_TO_IDR_RATE;

                return amount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting currency: {ex.Message}");
                return amount;
            }

        }

        /// <summary>
        /// Notifies subscribers when the budget is updated.
        /// </summary>
        public static void NotifyBudgetUpdated()
        {
            Console.WriteLine("BudgetUpdated event triggered.");
            BudgetUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Represents an individual expense.
    /// </summary>
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
