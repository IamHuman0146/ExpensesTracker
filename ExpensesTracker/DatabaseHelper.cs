using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ExpensesTracker
{
    public static class DatabaseHelper
    {
        private static readonly string connectionString = @"Data Source=C:\sqlite\db\ExpensesTracker;Version=3;";

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
                        Gender TEXT,
                        Income REAL DEFAULT 0
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
                ", connection);
                command.ExecuteNonQuery();
            }
        }

        public static bool ValidateUser(string email, string password, out int userId, out int age, out string gender, out decimal income)
        {
            userId = 0;
            age = 0;
            gender = string.Empty;
            income = 0;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT UserID, Age, Gender, Income FROM Users WHERE Email = @Email AND Password = @Password",
                    connection);

                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                        age = reader.GetInt32(1);
                        gender = reader.GetString(2);
                        income = reader.GetDecimal(3);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool AddUser(string email, string password, int age, string gender)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SQLiteCommand(
                        "INSERT INTO Users (Email, Password, Age, Gender, Income) VALUES (@Email, @Password, @Age, @Gender, @Income)",
                        connection);

                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Age", age);
                    command.Parameters.AddWithValue("@Gender", gender);
                    command.Parameters.AddWithValue("@Income", 0); // Default income is 0

                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding user: {ex.Message}");
                return false;
            }
        }

        public static void UpdateIncome(int userId, decimal newIncome)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "UPDATE Users SET Income = @Income WHERE UserID = @UserID",
                    connection);

                command.Parameters.AddWithValue("@Income", newIncome);
                command.Parameters.AddWithValue("@UserID", userId);

                command.ExecuteNonQuery();
            }
        }

        public static int GetUserIdByEmail(string email)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(
                    "SELECT UserID FROM Users WHERE Email = @Email",
                    connection);

                command.Parameters.AddWithValue("@Email", email);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        public static List<Expense> GetExpenses(int userId)
        {
            var expenses = new List<Expense>();

            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
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
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine($"SQLite Error: {ex.Message}");
            }

            return expenses;
        }


        public static bool AddExpense(int userId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = new SQLiteCommand(@"
                INSERT INTO Expenses (UserID, ItemName, ItemType, AmountSpent, Currency, Date)
                VALUES (@UserID, @ItemName, @ItemType, @AmountSpent, @Currency, @Date)", connection);

                    command.Parameters.AddWithValue("@UserID", userId); // Ensure the correct UserID is used
                    command.Parameters.AddWithValue("@ItemName", itemName);
                    command.Parameters.AddWithValue("@ItemType", itemType);
                    command.Parameters.AddWithValue("@AmountSpent", amountSpent);
                    command.Parameters.AddWithValue("@Currency", currency);
                    command.Parameters.AddWithValue("@Date", date);

                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding expense: {ex.Message}");
                return false;
            }
        }


        public static void UpdateExpense(int expenseId, string itemName, string itemType, decimal amountSpent, string currency, string date)
        {
            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = new SQLiteCommand(@"
                UPDATE Expenses
                SET ItemName = @ItemName, ItemType = @ItemType, AmountSpent = @AmountSpent, Currency = @Currency, Date = @Date
                WHERE ExpenseID = @ExpenseID", connection);

                    command.Parameters.AddWithValue("@ExpenseID", expenseId);
                    command.Parameters.AddWithValue("@ItemName", itemName);
                    command.Parameters.AddWithValue("@ItemType", itemType);
                    command.Parameters.AddWithValue("@AmountSpent", amountSpent);
                    command.Parameters.AddWithValue("@Currency", currency);
                    command.Parameters.AddWithValue("@Date", date);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating expense: {ex.Message}");
            }
        }

        public static void DeleteExpense(int expenseId)
        {
            try
            {
                using (var connection = new SQLiteConnection(GetConnectionString()))
                {
                    connection.Open();
                    var command = new SQLiteCommand("DELETE FROM Expenses WHERE ExpenseID = @ExpenseID", connection);

                    command.Parameters.AddWithValue("@ExpenseID", expenseId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting expense: {ex.Message}");
            }
        }

    }
}
