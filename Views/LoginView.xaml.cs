using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class LoginView : UserControl
{
    private readonly UserService _userService;

    public LoginView(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var result = _userService.Login(UsernameOrEmailTextBox.Text, PasswordBox.Password, RememberMeCheckBox.IsChecked == true);
        MessageTextBlock.Text = result.Message;
        if (result.Success)
        {
            UsernameOrEmailTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            RememberMeCheckBox.IsChecked = false;

            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.MainContent.Content = new TextBlock { Text = "Ласкаво просимо назад!", FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                mainWindow.UpdateButtonsVisibility();
            }
        }
    }
}
