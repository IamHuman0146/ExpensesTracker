using System;
using System.IO;
using System.Text.Json;

namespace ExpensesTracker
{
    public static class BudgetHelper
    {
        private static string GetFilePath(int userId)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Budget_{userId}.json");
        }

        public static void SaveBudget(int userId, decimal budget)
        {
            try
            {
                string filePath = GetFilePath(userId);
                var budgetData = new BudgetData { UserId = userId, CalculatedBudget = budget };
                string json = JsonSerializer.Serialize(budgetData);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving budget: {ex.Message}");
            }
        }
        
        public static decimal LoadBudget(int userId)
        {
            try
            {
                string filePath = GetFilePath(userId);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var budgetData = JsonSerializer.Deserialize<BudgetData>(json);
                    return budgetData?.CalculatedBudget ?? 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading budget: {ex.Message}");
            }
            return 0; // Default budget if not found
        }
    }

    public class BudgetData
    {
        public int UserId { get; set; }
        public decimal CalculatedBudget { get; set; }
    }
}
