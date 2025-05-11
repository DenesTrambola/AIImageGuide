using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class SearchView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;
    private int _currentPage = 1;
    private int _pageSize = 6;
    private int _totalPages = 1;

    public SearchView(ImageService imageService, UserService userService)
    {
        InitializeComponent();
        _imageService = imageService;
        _userService = userService;

        var categories = _imageService.GetCategories().ToList();
        categories.Insert(0, new() { Id = -1, Name = "All Categories" });

        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedIndex = 0;

        RefreshSearchResults();
    }

    private void RefreshSearchResults()
    {
        int? categoryId = CategoryComboBox.SelectedIndex == 0 ? null : ((dynamic)CategoryComboBox.SelectedItem).Id;
        var (images, totalPages) = _imageService.SearchImages(SearchTextBox.Text, categoryId, _currentPage, _pageSize);

        _totalPages = totalPages;
        SearchResultsListView.ItemsSource = images;

        UpdatePaginationControls();
    }

    private void UpdatePaginationControls()
    {
        PageInfoTextBlock.Text = $"Page {_currentPage} of {_totalPages}";
        PreviousButton.IsEnabled = _currentPage > 1;
        NextButton.IsEnabled = _currentPage < _totalPages;

        if (_totalPages == 0)
            PageInfoTextBlock.Text = "No images found.";
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            RefreshSearchResults();
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            RefreshSearchResults();
        }
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _currentPage = 1; // Reset to first page on search change
        RefreshSearchResults();
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _currentPage = 1; // Reset to first page on category change
        RefreshSearchResults();
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
