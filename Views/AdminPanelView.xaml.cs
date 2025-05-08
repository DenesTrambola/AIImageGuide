using AIImageGuide.Models;
using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class AdminPanelView : UserControl
{
    private readonly AdminService _adminService;

    public AdminPanelView(AdminService adminService)
    {
        InitializeComponent();
        _adminService = adminService;
        RefreshUsers();
    }

    private void RefreshUsers()
    {
        UsersDataGrid.ItemsSource = _adminService.GetAllUsers();
    }

    private void BlockButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is User user)
        {
            _adminService.BlockUser(user.Id, !user.IsBlocked);
            RefreshUsers();
        }
    }

    private void ToggleRoleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is User user)
        {
            string newRole = user.Role == "Admin" ? "Registered" : "Admin";
            _adminService.ChangeRole(user.Id, newRole);
            RefreshUsers();
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is User user)
        {
            _adminService.DeleteUser(user.Id);
            RefreshUsers();
        }
    }
}
