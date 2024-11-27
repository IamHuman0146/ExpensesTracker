using System;
using System.Windows;

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

            int userId, age;
            string gender;
            decimal income;

            if (DatabaseHelper.ValidateUser(email, password, out userId, out age, out gender, out income))
            {
                // Update the logged-in user's data
                ExpenseDataStore.UserID = userId;
                ExpenseDataStore.UserEmail = email;
                ExpenseDataStore.UserAge = age;
                ExpenseDataStore.UserGender = gender;
                ExpenseDataStore.Income = income;
<<<<<<< HEAD

                // Initialize ExpenseDataStore with user-specific data
                ExpenseDataStore.InitializeDataStore(userId);
=======
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d

                MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Navigate to the MainPage
                MainPage mainPage = new MainPage
                {
                    WindowState = WindowState.Maximized,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize
                };
                mainPage.Show();

                // Close the login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


<<<<<<< HEAD


=======
>>>>>>> 45778210c2e9f0c544850818ad87abe921edda3d
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

        private void SignUpText_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Replace the content with SignUpPage
            this.Content = new SignUpPage();
        }

        private void GoogleLogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Google login is not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

    }
}
