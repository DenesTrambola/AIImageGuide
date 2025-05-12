using AIImageGuide.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AIImageGuide.Views;

public partial class ImageDetailsView : UserControl
{
    private readonly ImageService _imageService;
    private readonly UserService _userService;
    private readonly int _imageId;

    public ImageDetailsView(ImageService imageService, UserService userService, int imageId)
    {
        InitializeComponent();
        _imageService = imageService;
        _userService = userService;
        _imageId = imageId;
        LoadImageDetails();
    }

    private void LoadImageDetails()
    {
        var image = _imageService.GetImages().Images.FirstOrDefault(i => i.Id == _imageId);

        if (image == null)
        {
            TitleTextBlock.Text = "Зображення не знайдено.";
            return;
        }

        ImageControl.Source = new BitmapImage(new Uri(image.FilePath));
        TitleTextBlock.Text = image.Title;
        DescriptionTextBlock.Text = image.Description ?? "Нема опису.";
        CommentsListView.ItemsSource = image.Comments;
    }

    private void RatingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
        {
            RatingMessageTextBlock.Text = "Будь ласка, увійдіть, щоб оцінити.";
            RatingComboBox.SelectedIndex = -1;
            return;
        }

        if (RatingComboBox.SelectedItem is ComboBoxItem selectedItem && int.TryParse(selectedItem.Content.ToString(), out int rating))
        {
            var result = _imageService.AddRating(_imageId, currentUser.Id, rating);
            RatingMessageTextBlock.Text = result.Message;
            RatingComboBox.SelectedIndex = -1;
        }
    }

    private void SubmitCommentButton_Click(object sender, RoutedEventArgs e)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
        {
            CommentMessageTextBlock.Text = "Будь ласка, увійдіть, щоб залишити коментар.";
            return;
        }

        var result = _imageService.AddComment(_imageId, currentUser.Id, CommentTextBox.Text);
        CommentMessageTextBlock.Text = result.Message;
        if (result.Success)
        {
            CommentTextBox.Text = string.Empty;
            LoadImageDetails();
        }
    }
}
