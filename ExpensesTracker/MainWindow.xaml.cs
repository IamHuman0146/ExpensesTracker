using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExpensesTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Set the window to fullscreen on startup
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize; // Disable resizing
            this.WindowStyle = WindowStyle.None; // Hide the window title bar
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            int age;
            string gender;

            if (DatabaseHelper.ValidateUser(email, password, out age, out gender)) // Correct number of arguments
            {
                ExpenseDataStore.UserEmail = email;
                ExpenseDataStore.UserAge = age;
                ExpenseDataStore.UserGender = gender;

                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Open MainPage and close this login window
                MainPage mainPage = new MainPage();
                mainPage.WindowState = WindowState.Maximized; // Ensure MainPage opens in fullscreen
                mainPage.Show();

                // Close the login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SignUpText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Content = new SignUpPage();
        }

        private void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Google login is not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
