using AIImageGuide.Models;
using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class UserProfileView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;
    private readonly int _userId;
    private int _currentPage = 1;
    private int _totalPages = 1;

    public UserProfileView(ImageService imageService, UserService userService, int userId)
    {
        InitializeComponent();
        _imageService = imageService;
        _userService = userService;
        _userId = userId;
        LoadProfile();
    }

    private void LoadProfile()
    {
        var user = _userService.GetUserById(_userId);
        if (user == null)
        {
            UsernameTextBlock.Text = "Користувача не знайдено.";
            return;
        }

        UploadedImagesListView.ItemsSource = _userService.GetUserImages(_userId);
        UsernameTextBlock.Text = user.Username;

        var ratings = _userService.GetUserRatings(_userId)
            .Select(r => new UserActivity() { Type = "Оцінка", Content = $"{r.Value}/5", Image = r.Image });

        var comments = _userService.GetUserComments(_userId)
            .Select(c => new UserActivity() { Type = "Прокоментував", Content = c.Content, Image = c.Image });

        ActivityListView.ItemsSource = ratings
            .Union(comments)
            .ToList();

        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {

        PageInfoTextBlock.Text = $"{_currentPage}/{_totalPages}";
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;

        if (_totalPages == 0)
            PageInfoTextBlock.Text = "Зображень не знайдено.";
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            LoadProfile();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            LoadProfile();
        }
    }

    private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Models.Image image)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
                mainWindow.NavigateToImageDetails(image.Id);
        }
    }

    private void DeleteImageButton_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
        {
            MessageBox.Show("Будь ласка, увійдіть, щоб видалити зображення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (sender is Button button && button.DataContext is Models.Image image)
        {
            var deleteFromDbResult = _imageService.DeleteImageFromDb(image.Id, currentUser.Id, currentUser.Role == "Admin");
            if (deleteFromDbResult.Success)
                UploadedImagesListView.ItemsSource = null;
            MessageBox.Show(deleteFromDbResult.Message, deleteFromDbResult.Success ? "Успіх" : "Помилка", MessageBoxButton.OK, deleteFromDbResult.Success ? MessageBoxImage.Information : MessageBoxImage.Error);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            _imageService.DeleteImageFromFileSystem(image.FilePath);

            LoadProfile();
        }
    }
}
