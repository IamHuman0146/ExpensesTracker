using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class AISavingTipsPage : Page
    {
        private const int MAX_RETRIES = 3;                  // Maximum retries for API call
        private const int DELAY_BETWEEN_RETRIES = 1000;     // Delay between retries in milliseconds

        public AISavingTipsPage()
        {
            InitializeComponent();
            LoadLastGeneratedTips();
        }

        private void LoadLastGeneratedTips()
        {
            // Load last generated tips from the data store if available
            if (!string.IsNullOrEmpty(ExpenseDataStore.LastGeneratedTips))
            {
                GeneratedTipsTextBox.Text = ExpenseDataStore.LastGeneratedTips;
            }
        }

        private async void GenerateTips_Click(object sender, RoutedEventArgs e)
        {
            var generateButton = sender as Button;
            if (generateButton != null)
                generateButton.IsEnabled = false;

            try
            {
                if (ExpenseDataStore.ApiCallCount >= ExpenseDataStore.ApiLimit)
                {
                    ApiUsageNotification.Text = "Daily API limit reached. Please try again tomorrow.";
                    return;
                }

                GeneratedTipsTextBox.Text = "Generating financial tips...";
                string tips = await GenerateTipsWithRetry();

                if (!string.IsNullOrEmpty(tips))
                {
                    ExpenseDataStore.ApiCallCount++;
                    ExpenseDataStore.LastGeneratedTips = tips;
                    GeneratedTipsTextBox.Text = tips;
                    ApiUsageNotification.Text = $"API calls used today: {ExpenseDataStore.ApiCallCount}/{ExpenseDataStore.ApiLimit}";
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                if (generateButton != null)
                    generateButton.IsEnabled = true;
            }
        }

        private async Task<string> GenerateTipsWithRetry()
        {
            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    string prompt = BuildPrompt();
                    string tips = await OpenAIClient.GetSavingTipsAsync(prompt);
                    return FormatTips(tips);
                }
                catch (Exception ex)
                {
                    if (attempt == MAX_RETRIES)
                        throw;  // Throw the exception if it's the last attempt

                    await Task.Delay(DELAY_BETWEEN_RETRIES * attempt);
                }
            }
            return null;
        }

        private string BuildPrompt()
        {
            var sb = new StringBuilder();
            string currency = ExpenseDataStore.CurrencySymbol;
            string formattedIncome = $"{currency} {ExpenseDataStore.Income:N0}";

            // Calculate total expenses and spending ratio
            decimal totalExpenses = ExpenseDataStore.Expenses.Sum(e => e.AmountSpent);
            decimal spendingRatio = (totalExpenses / ExpenseDataStore.Income) * 100;

            // Expense breakdown sorted by amount spent
            var expenseBreakdown = ExpenseDataStore.Expenses
                .OrderByDescending(e => e.AmountSpent)
                .Select(e => $"{e.ItemName}: {currency} {e.AmountSpent:N0}");

            // Build the prompt text
            sb.AppendLine("Provide a financial summary under 120 words based on this information:");
            sb.AppendLine($"\nIncome: {formattedIncome}");
            sb.AppendLine($"Total Expenses: {currency} {totalExpenses:N0} ({spendingRatio:F1}% of income)");
            sb.AppendLine("Expense Breakdown:");
            foreach (var expense in expenseBreakdown)
            {
                sb.AppendLine($"- {expense}");
            }

            sb.AppendLine("\nPlease include:");
            sb.AppendLine("1. A quick financial health assessment.");
            sb.AppendLine("2. Practical saving tips based on the expenses.");

            if (spendingRatio > 75)
            {
                sb.AppendLine("\nNote: Expenses are above 75% of income. Focus on cost-reduction strategies.");
            }

            return sb.ToString();
        }

        private string FormatTips(string rawTips)
        {
            if (string.IsNullOrEmpty(rawTips))
                return "No tips generated. Please try again.";

            // Process and format tips
            return rawTips
                .Replace("\n\n", "\n")               // Replace double new lines
                .Replace("\\n", "\n")                // Replace escaped new lines
                .Replace("*", "")                    // Remove bullet points if present
                .Trim()
                .Split('\n')
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Aggregate(new StringBuilder(), (sb, line) =>
                {
                    sb.AppendLine(line.Trim());
                    return sb;
                })
                .ToString();
        }

        private void HandleError(Exception ex)
        {
            string errorMessage;
            if (ex.Message.Contains("rate limit"))
            {
                errorMessage = "API rate limit reached. Please try again in a few minutes.";
            }
            else if (ex.Message.Contains("token"))
            {
                errorMessage = "Response too long. Please try again.";
            }
            else
            {
                errorMessage = "Error generating tips. Please try again later.";
            }

            GeneratedTipsTextBox.Text = errorMessage;
            ApiUsageNotification.Text = errorMessage;
        }
    }
}
