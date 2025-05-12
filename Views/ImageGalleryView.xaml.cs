using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class ImageGalleryView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;
    private int _currentPage = 1;
    private int _pageSize = 6;
    private int _totalPages = 1;

    public ImageGalleryView(ImageService imageService, UserService userService)
    {
        InitializeComponent();
        _imageService = imageService;
        _userService = userService;

        var categories = _imageService.GetCategories().ToList();
        categories.Insert(0, new() { Id = 0, Name = "Усі категорії" });

        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedIndex = 0;
        SortComboBox.SelectedIndex = 0;

        RefreshImages();
    }

    private void RefreshImages()
    {
        int? categoryId = CategoryComboBox.SelectedIndex == 0 ? null : ((dynamic)CategoryComboBox.SelectedItem).Id;
        string sortBy = SortComboBox.SelectedIndex == 0 ? "UploadDate" : "Rating";
        var (images, totalPages) = _imageService.GetImages(categoryId, sortBy, _currentPage, _pageSize);

        _totalPages = totalPages;
        ImagesListView.ItemsSource = images;

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
            RefreshImages();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            RefreshImages();
        }
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentPage = 1; // Reset to first page on filter change
        RefreshImages();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentPage = 1; // Reset to first page on sort change
        RefreshImages();
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
}
