using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AIImageGuide.Views;

public partial class RegisterView : UserControl
{
    private readonly UserService _userService;

    public RegisterView(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var result = _userService.Register(UsernameTextBox.Text, EmailTextBox.Text, PasswordBox.Password);
        MessageTextBlock.Foreground = result.Success ? Brushes.Green : Brushes.Red;
        MessageTextBlock.Text = result.Message;
        if (result.Success)
        {
            UsernameTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
        }
    }
}
