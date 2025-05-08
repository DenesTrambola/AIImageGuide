using AIImageGuide.Models;
using AIImageGuide.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace AIImageGuide.ViewModels;

public class AdminPanelViewModel : INotifyPropertyChanged
{
    private readonly AdminService _adminService;
    private ObservableCollection<User> _users;

    public ObservableCollection<User> Users
    {
        get => _users;
        set { _users = value; OnPropertyChanged(nameof(Users)); }
    }

    public ICommand BlockCommand { get; }
    public ICommand ChangeRoleCommand { get; }
    public ICommand DeleteCommand { get; }

    public AdminPanelViewModel(AdminService adminService)
    {
        _adminService = adminService;
        Users = new ObservableCollection<User>(_adminService.GetAllUsers());
        BlockCommand = new RelayCommand(BlockUser);
        ChangeRoleCommand = new RelayCommand(ChangeRole);
        DeleteCommand = new RelayCommand(DeleteUser);
    }

    private void BlockUser(object parameter)
    {
        if (parameter is User user)
        {
            _adminService.BlockUser(user.Id, !user.IsBlocked);
            RefreshUsers();
        }
    }

    private void ChangeRole(object parameter)
    {
        if (parameter is User user)
        {
            string newRole = user.Role == "Admin" ? "Registered" : "Admin";
            _adminService.ChangeRole(user.Id, newRole);
            RefreshUsers();
        }
    }

    private void DeleteUser(object parameter)
    {
        if (parameter is User user)
        {
            _adminService.DeleteUser(user.Id);
            RefreshUsers();
        }
    }

    private void RefreshUsers()
    {
        Users.Clear();
        foreach (var user in _adminService.GetAllUsers())
            Users.Add(user);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
