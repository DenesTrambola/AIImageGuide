using AIImageGuide.Data;
using AIImageGuide.Services;
using AIImageGuide.Views;
using HandyControl.Themes;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Media;

namespace AIImageGuide;

public partial class MainWindow : Window
{
    private bool isDarkTheme = true;

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

        UpdateButtonsVisibility();

        MainContent.Content = _userService.CurrentUser == null
            ? new LoginView(_userService)
            : new ImageGalleryView(_imageService, _userService);

        UpdateTheme();
    }

    public void UpdateButtonsVisibility()
    {
        if (_userService.CurrentUser == null)
        {
            LoginButton.Visibility = Visibility.Visible;
            RegisterButton.Visibility = Visibility.Visible;
            LogoutButton.Visibility = Visibility.Collapsed;
            ProfileButton.Visibility = Visibility.Collapsed;
            UploadButton.Visibility = Visibility.Collapsed;
            AdminPanelButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            LoginButton.Visibility = Visibility.Collapsed;
            RegisterButton.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Visible;
            ProfileButton.Visibility = Visibility.Visible;
            UploadButton.Visibility = Visibility.Visible;
            AdminPanelButton.Visibility = _userService.CurrentUser.Role == "Admin" ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public void NavigateToImageDetails(int imageId)
    {
        MainContent.Content = new ImageDetailsView(_imageService, _userService, imageId);
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
            MessageBox.Show("Будь ласка, увійдіть, щоб переглянути свій профіль.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            MainContent.Content = new LoginView(_userService);
        }
        else
        {
            MainContent.Content = new UserProfileView(_imageService, _userService, currentUser.Id);
        }
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
            MessageBox.Show("Доступ заборонено. Потрібна роль адміністратора..", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _userService.Logout();
        UpdateButtonsVisibility();
        MainContent.Content = new LoginView(_userService);
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        UpdateTheme();
    }

    private void UpdateTheme()
    {
        if (isDarkTheme)
        {
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            Application.Current.Resources["AppBackground"] = new SolidColorBrush(Color.FromRgb(237, 246, 249));
            Application.Current.Resources["AppForeground"] = new SolidColorBrush(Colors.Black);
            isDarkTheme = false;
        }
        else
        {
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            Application.Current.Resources["AppBackground"] = new SolidColorBrush(Color.FromRgb(30, 30, 45));
            Application.Current.Resources["AppForeground"] = new SolidColorBrush(Colors.White);
            isDarkTheme = true;
        }
    }
}
