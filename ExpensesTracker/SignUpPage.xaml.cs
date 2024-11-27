using System;
using System.Windows;
using System.Windows.Controls;

namespace ExpensesTracker
{
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string ageText = AgeTextBox.Text;
            string gender = MaleRadioButton.IsChecked == true ? "Male" : FemaleRadioButton.IsChecked == true ? "Female" : null;

            // Validate email domain
            if (!email.EndsWith("@example.com"))
            {
                MessageBox.Show("Email must be in the format of @example.com", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate age input
            if (string.IsNullOrWhiteSpace(ageText))
            {
                MessageBox.Show("Please enter your age.", "Age Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ageText, out int age))
            {
                MessageBox.Show("Age must be a valid number.", "Invalid Age", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (age < 18)
            {
                MessageBox.Show("You must be at least 18 years old to register.", "Age Restriction", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate gender selection
            if (string.IsNullOrEmpty(gender))
            {
                MessageBox.Show("Please select a gender.", "Gender Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add user to the database
            if (DatabaseHelper.AddUser(email, password, age, gender))
            {
                MessageBox.Show("Account created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Email already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Back button click event to navigate to the dashboard window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardWindow dashboardWindow = new DashboardWindow();
            dashboardWindow.Show();
            Window.GetWindow(this)?.Close();
        }
    }
}
