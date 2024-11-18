using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ExpensesTracker
{
    public static class DatabaseHelper
    {
        // Keep connectionString private
        private static readonly string connectionString = @"Data Source=C:\sqlite\db\ExpensesTracker;Version=3;";

        // Provide a public method to access the connection string
        public static string GetConnectionString()
        {
            return connectionString;
        }

        public static void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Email TEXT NOT NULL UNIQUE,
                        Password TEXT NOT NULL,
                        Age INTEGER NOT NULL,
                        Gender TEXT
                    );
                    CREATE TABLE IF NOT EXISTS Expenses (
                        ExpenseID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserID INTEGER NOT NULL,
                        ItemName TEXT NOT NULL,
                        ItemType TEXT NOT NULL,
                        AmountSpent REAL NOT NULL,
                        Currency TEXT NOT NULL,
                        Date TEXT NOT NULL,
                        FOREIGN KEY (UserID) REFERENCES Users(UserID)
                    );
                    CREATE TABLE IF NOT EXISTS Income (
                        IncomeID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserID INTEGER NOT NULL,
                        Amount REAL NOT NULL,
                        Currency TEXT NOT NULL,
                        Date TEXT NOT NULL,
                        FOREIGN KEY (UserID) REFERENCES Users(UserID)
                    );", connection);
                command.ExecuteNonQuery();
            }
        }

        public static bool AddUser(string email, string password, int age, string gender)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SQLiteCommand("INSERT INTO Users (Email, Password, Age, Gender) VALUES (@Email, @Password, @Age, @Gender)", connection);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@Gender", gender);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                return false;
            }
        }

        public static bool ValidateUser(string email, string password, out int age, out string gender)
        {
            age = 0;
            gender = string.Empty;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT Age, Gender FROM Users WHERE Email = @Email AND Password = @Password", connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    age = reader.GetInt32(0);
                    gender = reader.GetString(1);
                    return true;
                }
                return false;
            }
        }

        public static bool AddExpense(int userId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SQLiteCommand(@"
                        INSERT INTO Expenses (UserID, ItemName, ItemType, AmountSpent, Currency, Date) 
                        VALUES (@UserID, @ItemName, @ItemType, @AmountSpent, @Currency, @Date)", connection);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@ItemName", itemName);
                    command.Parameters.AddWithValue("@ItemType", itemType);
                    command.Parameters.AddWithValue("@AmountSpent", amountSpent);
                    command.Parameters.AddWithValue("@Currency", currency);
                    command.Parameters.AddWithValue("@Date", date);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Error adding expense: {ex.Message}");
                return false;
            }
        }

        public static List<Expense> GetExpenses(int userId)
        {
            var expenses = new List<Expense>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand("SELECT * FROM Expenses WHERE UserID = @UserID", connection);
                command.Parameters.AddWithValue("@UserID", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(new Expense
                        {
                            ExpenseID = reader.GetInt32(reader.GetOrdinal("ExpenseID")),
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
                            ItemType = reader.GetString(reader.GetOrdinal("ItemType")),
                            AmountSpent = reader.GetDecimal(reader.GetOrdinal("AmountSpent")),
                            Currency = reader.GetString(reader.GetOrdinal("Currency")),
                            Date = reader.GetString(reader.GetOrdinal("Date"))
                        });
                    }
                }
            }

            return expenses;
        }

        public static bool UpdateExpense(int expenseId, int userId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SQLiteCommand(@"
                        UPDATE Expenses SET ItemName = @ItemName, ItemType = @ItemType, AmountSpent = @AmountSpent, Currency = @Currency, Date = @Date 
                        WHERE ExpenseID = @ExpenseID AND UserID = @UserID", connection);
                    command.Parameters.AddWithValue("@ExpenseID", expenseId);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@ItemName", itemName);
                    command.Parameters.AddWithValue("@ItemType", itemType);
                    command.Parameters.AddWithValue("@AmountSpent", amountSpent);
                    command.Parameters.AddWithValue("@Currency", currency);
                    command.Parameters.AddWithValue("@Date", date);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Error updating expense: {ex.Message}");
                return false;
            }
        }

        public static bool DeleteExpense(int expenseId, int userId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SQLiteCommand("DELETE FROM Expenses WHERE ExpenseID = @ExpenseID AND UserID = @UserID", connection);
                    command.Parameters.AddWithValue("@ExpenseID", expenseId);
                    command.Parameters.AddWithValue("@UserID", userId);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"Error deleting expense: {ex.Message}");
                return false;
            }
        }
    }
}
