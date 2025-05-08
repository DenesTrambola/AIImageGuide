using AIImageGuide.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace AIImageGuide.ViewModels;

public class RegisterViewModel : INotifyPropertyChanged
{
    private readonly UserService _userService;
    private string _username;
    private string _email;
    private string _password;
    private string _message;

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(nameof(Username)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(nameof(Password)); }
    }

    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(nameof(Message)); }
    }

    public ICommand RegisterCommand { get; }

    public RegisterViewModel(UserService userService)
    {
        _userService = userService;
        RegisterCommand = new RelayCommand(Register);
    }

    private void Register(object parameter)
    {
        var result = _userService.Register(Username, Email, Password);
        Message = result.Message;
        if (result.Success)
        {
            Username = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
