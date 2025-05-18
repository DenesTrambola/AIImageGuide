using AIImageGuide.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AIImageGuide.Views;

public partial class ImageUploadView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;
    private string _filePath;

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
            _filePath = openFileDialog.FileName;
            FileControl.Source = new BitmapImage(new Uri(_filePath));
            FileControl.Visibility = Visibility.Visible;
        }
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
        {
            MessageTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            MessageTextBlock.Text = "Будь ласка, увійдіть, щоб завантажити зображення.";
            return;
        }

        if (CategoryComboBox.SelectedItem == null)
        {
            MessageTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            MessageTextBlock.Text = "Будь ласка, виберіть категорію.";
            return;
        }

        var result = _imageService.UploadImage(
            TitleTextBox.Text,
            DescriptionTextBox.Text,
            ((Models.Category)CategoryComboBox.SelectedItem).Id,
            _filePath,
            currentUser.Id
        );

        MessageTextBlock.Foreground = result.Success ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        MessageTextBlock.Text = result.Message;
        if (result.Success)
        {
            TitleTextBox.Text = string.Empty;
            DescriptionTextBox.Text = string.Empty;
            CategoryComboBox.SelectedIndex = -1;
            FileControl.Visibility = Visibility.Collapsed;
            _filePath = null;
        }
    }
}
