using System.Windows;
using System.Windows.Input;
using System.Windows.Controls; // Required for Frame

namespace ExpensesTracker
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized; // Ensure the window starts maximized
        }

        // Allows the window to be moved by dragging the top panel
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeRestoreWindow();
            }
            else
            {
                DragMove();
            }
        }

        // Minimize the window
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Maximize or restore the window
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeRestoreWindow();
        }

        // Method to toggle between maximized and normal window states
        private void MaximizeRestoreWindow()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        // Close the window
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Log In button click event - navigates to the MainWindow (Login page)
        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }

        // Sign Up text click event - navigates to the SignUpPage in the current window
        private void SignUpText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Using a Frame to navigate to SignUpPage within the current window
            Frame contentFrame = new Frame();
            contentFrame.Navigate(new SignUpPage());
            this.Content = contentFrame;
        }
    }
}
