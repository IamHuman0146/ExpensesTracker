using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ExpensesTracker
{
    public partial class MainPage : Window
    {
        public MainPage()
        {
            InitializeComponent();
            ExpenseDataStore.LoadExpenses();
            ExpenseDataStore.UpdateCalculatedBudget();
            this.Closed += OnMainWindowClosed;
        }

        // Helper method to reset the background of all buttons
        private void ResetButtonStyles()
        {
            ExpensesButton.Background = Brushes.Transparent;
            AnalysisButton.Background = Brushes.Transparent;
            SavingTipsButton.Background = Brushes.Transparent;
            SetAchievementsButton.Background = Brushes.Transparent;
            ViewAchievementsButton.Background = Brushes.Transparent;
        }

        private void NavigateToExpenses(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            ExpensesButton.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226)); 
            MainContentFrame.Navigate(new ExpensesPage());
        }

        private void NavigateToAnalysis(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            AnalysisButton.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226)); 
            MainContentFrame.Navigate(new ViewAnalysisPage());
        }

        private void NavigateToSavingTips(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            SavingTipsButton.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226));
            MainContentFrame.Navigate(new AISavingTipsPage());
        }

        private void NavigateToSetAchievements(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            SetAchievementsButton.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226));
            MainContentFrame.Navigate(new SetFinancialAchievementsPage());
        }

        private void NavigateToViewAchievements(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            ViewAchievementsButton.Background = new SolidColorBrush(Color.FromRgb(74, 144, 226));

            // Ensure the budget is recalculated before navigating
            ExpenseDataStore.UpdateCalculatedBudget();

            MainContentFrame.Navigate(new ViewFinancialAchievementsPage());
        }


        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Do you want to log out?", "Navigate", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Navigate to Dashboard instead of Login
                DashboardWindow dashboardWindow = new DashboardWindow();
                dashboardWindow.Show();

                // Close the current MainPage window
                this.Close();
            }
        }
        private void OnMainWindowClosed(object sender, EventArgs e)
        {
            // Reset the budget to its initial state
            ExpenseDataStore.CalculatedBudget = ExpenseDataStore.InitialBudget;

            // Trigger any recalculations or updates if necessary
            ExpenseDataStore.UpdateCalculatedBudget();

            // Optionally log or debug to confirm the method is called
            Console.WriteLine("Main window closed: Budget reset.");
        }


    }
}
