using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class ImageGalleryView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;

    public ImageGalleryView(ImageService imageService, UserService userService)
    {
        InitializeComponent();
        _imageService = imageService;
        _userService = userService;

        var categories = _imageService.GetCategories().ToList();
        categories.Insert(0, new Models.Category { Id = -1, Name = "All Categories" });

        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedIndex = 0;
        SortComboBox.SelectedIndex = 0;

        RefreshImages();
    }

    private void RefreshImages()
    {
        int? categoryId = CategoryComboBox.SelectedIndex == 0 ? null : ((dynamic)CategoryComboBox.SelectedItem).Id;
        string sortBy = SortComboBox.SelectedIndex == 0 ? "UploadDate" : "Rating";
        ImagesListView.ItemsSource = _imageService.GetImages(categoryId, sortBy);
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshImages();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshImages();
    }

    private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Models.Image image)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.NavigateToImageDetails(image.Id);
        }
    }
}
