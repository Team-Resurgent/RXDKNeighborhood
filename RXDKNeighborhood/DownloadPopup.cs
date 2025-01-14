using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using RXDKXBDM.Models;
using System.Threading;

namespace RXDKNeighborhood;

public class DownloadPopup : Popup
{
    private List<DriveItem> mDriveItems;
    private string mFolder;
    private Label mItemsRemainingLabel;
    private Label mDownloadingLabel;
    private CancellationToken mCancellationToken;

    public DownloadPopup(DriveItem[] driveItems, string folder)
    {
        mDriveItems = driveItems.ToList();
        mFolder = folder;
        mCancellationToken = new CancellationToken();

        var isDarkTheme = AppInfo.RequestedTheme == AppTheme.Dark;

        var backgroundColor = isDarkTheme ? Color.FromArgb("#1E1E1E") : Color.FromArgb("#F3F3F3");
        var borderColor = isDarkTheme ? Color.FromArgb("#3C3C3C") : Color.FromArgb("#D2D2D2");
        var shadowBrush = isDarkTheme ? Colors.Black.WithAlpha(0.4f) : Colors.Black.WithAlpha(0.15f);
        var textColor = isDarkTheme ? Colors.White : Colors.Black;

        Color = Colors.Transparent;
        HorizontalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center;
        VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center;
        CanBeDismissedByTappingOutsideOfPopup = false;

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 150 },
                new ColumnDefinition { Width = 300 } 
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        var usedSpaceColor = Colors.Blue;
        var freeSpaceColor = Colors.Purple;

        var itemsRemainingCaptionLabel = new Label
        {
            Text = "Items Remaining:",
            VerticalOptions = LayoutOptions.Center,
        };

        mItemsRemainingLabel = new Label
        {
            Text = "0",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var downloadingCaptionLabel = new Label
        {
            Text = "Downloading:",
            VerticalOptions = LayoutOptions.Center,
        };

        mDownloadingLabel = new Label
        {
            Text = "N/A",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        grid.Add(itemsRemainingCaptionLabel, 0, 0);
        grid.Add(mItemsRemainingLabel, 1, 0);
        grid.Add(downloadingCaptionLabel, 0, 1);
        grid.Add(mDownloadingLabel, 1, 1);

        var okButton = new Button
        {
            Text = "Cancel",
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalOptions = LayoutOptions.Start
        };
        okButton.Clicked += (sender, e) =>
        {
            Close();
        };

        var retryButton = new Button
        {
            Text = "Retry",
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalOptions = LayoutOptions.Start
        };
        retryButton.Clicked += RetryButton_Clicked;

        var stack = new HorizontalStackLayout
        {
            okButton,
            retryButton
        };

        grid.AddWithSpan(stack, 2, 0, 1, 2);

        var border = new Border
        {
            Stroke = borderColor,    
            StrokeThickness = 2,     
            StrokeShape = new RoundRectangle { CornerRadius = 5 }, 
            Shadow = new Shadow
            {
                Brush = shadowBrush,
                Offset = new Point(0, 4),
                Opacity = 0.2f,
                Radius = 8
            },
            BackgroundColor = backgroundColor, 
            Padding = 10,
            Content = grid   
        };

        Task.Run(() =>
        {
            if (mDriveItems.Count == 0)
            {
                return;
            }

            var sourceBase = mDriveItems[0].Path;
            //var rootFolder = mDriveItems[0].Name;
            //var destBase = "G:\\Test";

            while (mDriveItems.Count() > 0)
            {
                var driveItem = mDriveItems[0];

                var folder = driveItem.CombinePath();
                var destFolder = folder.Substring(sourceBase.Length);
                var destPath = System.IO.Path.Combine(mFolder, destFolder);

                if (driveItem.IsFile)
                {
                    Dispatcher.Dispatch(() =>
                    {
                        mDownloadingLabel.Text = driveItem.Name;
                    });

                    var success = Utils.DownloadFileAsync(driveItem.CombinePath(), destPath, mCancellationToken, (step, size) =>
                    {
                        //Dispatcher.Dispatch(() =>
                        //{
                        //    mSizeLabel.Text = Utils.FormatBytes(progress.TotalSize);
                        //    mComtentsLabel.Text = $"{progress.FilesCount} Files, {progress.FolderCount} Folders";
                        //});
                    }).Result;

                    if (success)
                    {
                        mDriveItems.RemoveAt(0);
                    }
                    else
                    {
                        // Retry
                        break;
                    }
                }
                else
                {
                    Directory.CreateDirectory(destPath);
                    mDriveItems.RemoveAt(0);
                }

                Dispatcher.Dispatch(() =>
                {
                    mItemsRemainingLabel.Text = mDriveItems.Count().ToString();
                });
            }

            Dispatcher.Dispatch(() =>
            {
                Close();
            });
        });

        Content = border;
    }

    private void RetryButton_Clicked(object? sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}