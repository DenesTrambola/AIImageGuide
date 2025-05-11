using AIImageGuide.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AIImageGuide.Views;

public partial class ImageUploadView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;
    private string _selectedFilePath;

    public ImageUploadView(ImageService imageService, UserService userService)
    {
        InitializeComponent();
        _imageService = imageService;
        _userService = userService;
        CategoryComboBox.ItemsSource = _imageService.GetCategories();
    }

    private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            _selectedFilePath = openFileDialog.FileName;
            FilePathTextBlock.Text = Path.GetFileName(_selectedFilePath);
        }
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
        {
            MessageTextBlock.Text = "Please log in to upload images.";
            return;
        }

        if (CategoryComboBox.SelectedItem == null)
        {
            MessageTextBlock.Text = "Please select a category.";
            return;
        }

        var result = _imageService.UploadImage(
            TitleTextBox.Text,
            DescriptionTextBox.Text,
            ((Models.Category)CategoryComboBox.SelectedItem).Id,
            _selectedFilePath,
            currentUser.Id
        );

        MessageTextBlock.Text = result.Message;
        if (result.Success)
        {
            TitleTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            CategoryComboBox.SelectedIndex = -1;
            FilePathTextBlock.Text = string.Empty;
            _selectedFilePath = null;
        }
    }
}
