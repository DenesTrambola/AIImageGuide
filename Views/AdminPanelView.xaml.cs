using AIImageGuide.Models;
using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class AdminPanelView : UserControl
{
    private readonly AdminService _adminService;
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int _totalPages = 1;

    public AdminPanelView(AdminService adminService)
    {
        InitializeComponent();
        _adminService = adminService;
        RefreshUsers();
    }

    private void RefreshUsers()
    {
        var (users, totalPages) = _adminService.GetAllUsers(_currentPage, _pageSize);
        _totalPages = totalPages;
        UsersDataGrid.ItemsSource = users;
        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PageInfoTextBlock.Text = $"Page {_currentPage} of {_totalPages}";
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;

        if (_totalPages == 0)
            PageInfoTextBlock.Text = "No users found.";
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            RefreshUsers();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            RefreshUsers();
        }
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
