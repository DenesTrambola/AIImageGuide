using AIImageGuide.Data;
using AIImageGuide.Services;
using AIImageGuide.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide;

public partial class MainWindow : Window
{
    private readonly UserService _userService;
    private readonly AdminService _adminService;
    private readonly ImageService _imageService;

    public MainWindow()
    {
        InitializeComponent();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=image-guide.db");
        var context = new AppDbContext(optionsBuilder.Options);

        _userService = new UserService(context);
        _adminService = new AdminService(context);
        _imageService = new ImageService(context);

        UpdateLogoutButtonVisibility();
        MainContent.Content = new LoginView(_userService);
    }

    public void UpdateLogoutButtonVisibility()
    {
        LogoutButton.Visibility = _userService.CurrentUser != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public void NavigateToImageDetails(int imageId)
    {
        MainContent.Content = new ImageDetailsView(_imageService, _userService, imageId);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new TextBlock { Text = "Welcome to AI Image Guide!", FontSize = 24, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new ImageUploadView(_imageService, _userService);
    }

    private void GalleryButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new ImageGalleryView(_imageService, _userService);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new SearchView(_imageService, _userService);
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show("Please log in to view your profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            MainContent.Content = new LoginView(_userService);
        }
        else
        {
            MainContent.Content = new UserProfileView(_imageService, _userService, currentUser.Id);
        }
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new LoginView(_userService);
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new RegisterView(_userService);
    }

    private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser?.Role == "Admin")
        {
            MainContent.Content = new AdminPanelView(_adminService);
        }
        else
        {
            MessageBox.Show("Access denied. Admin role required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _userService.Logout();
        UpdateLogoutButtonVisibility();
        MainContent.Content = new LoginView(_userService);
    }
}
