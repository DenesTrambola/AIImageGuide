using AIImageGuide.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace AIImageGuide.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;
    private string _usernameOrEmail;
    private string _password;
    private bool _rememberMe;
    private string _message;

    public event Action LoginSuccess;

    public string UsernameOrEmail
    {
        get => _usernameOrEmail;
        set { _usernameOrEmail = value; OnPropertyChanged(nameof(UsernameOrEmail)); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(nameof(Password)); }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set { _rememberMe = value; OnPropertyChanged(nameof(RememberMe)); }
    }

    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(nameof(Message)); }
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel(UserService userService)
    {
        _userService = userService;
        LoginCommand = new RelayCommand(Login);
    }

    private void Login(object parameter)
    {
        var result = _userService.Login(UsernameOrEmail, Password, RememberMe);
        Message = result.Message;
        if (result.Success)
        {
            // Navigate to main content (handled in MainWindow)
            UsernameOrEmail = string.Empty;
            Password = string.Empty;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
