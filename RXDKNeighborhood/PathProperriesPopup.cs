using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using RXDKXBDM.Commands;
using RXDKXBDM.Models;

namespace RXDKNeighborhood;

public class PathProperriesPopup : Popup
{
    private DriveItem mDriveItem;
    private Action mNeedsUpdate;

    public PathProperriesPopup(DriveItem driveItem, string ipAddress, Action needsUpdate)
    {
        mDriveItem = driveItem;
        mNeedsUpdate = needsUpdate;

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
                new ColumnDefinition { Width = 100 },
                new ColumnDefinition { Width = GridLength.Auto } 
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }

            }
        };

        var consoleIpCaptionLabel = new Label
        {
            Text = "Console IP:",
            VerticalOptions = LayoutOptions.Center,
        };

        var consoleIpLabel = new Label
        {
            Text = ipAddress,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var itemNameCaptionLabel = new Label
        {
            Text = "Name:",
            VerticalOptions = LayoutOptions.Center,
        };

        var itemNameLabel = new Label
        {
            Text = mDriveItem.Name,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var itemTypeCaptionLabel = new Label
        {
            Text = "Type:",
            VerticalOptions = LayoutOptions.Center,
        };

        var itemTypeLabel = new Label
        {
            Text = mDriveItem.IsDirectory ? "Directory" : "File",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var separatorLine1 = new BoxView
        {
            Color = Colors.Gray,
            Margin = new Thickness(0, 5, 0, 5),
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill
        };

        var locationCaptionLabel = new Label
        {
            Text = "Location:",
            VerticalOptions = LayoutOptions.Center,
        };

        var locationLabel = new Label
        {
            Text = mDriveItem.Path,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var sizeCaptionLabel = new Label
        {
            Text = "Size:",
            VerticalOptions = LayoutOptions.Center,
        };

        var sizeLabel = new Label
        {
            Text = mDriveItem.Size.ToString("N0") + " bytes",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var separatorLine2 = new BoxView
        {
            Color = Colors.Gray,
            Margin = new Thickness(0, 5, 0, 5),
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill
        };

        var createdCaptionLabel = new Label
        {
            Text = "Created:",
            VerticalOptions = LayoutOptions.Center,
        };

        var createdLabel = new Label
        {
            Text = mDriveItem.Created.ToString("dddd, MMMM dd, yyyy h:mm:ss tt"),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var modifiedCaptionLabel = new Label
        {
            Text = "Modified:",
            VerticalOptions = LayoutOptions.Center,
        };

        var modifiedLabel = new Label
        {
            Text = mDriveItem.Changed.ToString("dddd, MMMM dd, yyyy h:mm:ss tt"),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var readonlyCaptionLabel = new Label
        {
            Text = "Read-only:",
            VerticalOptions = LayoutOptions.Center,
        };

        var readonlyCheckbox = new CheckBox
        {
            IsChecked = mDriveItem.IsReadOnly,
        };
        readonlyCheckbox.CheckedChanged += Readonly_CheckedChanged;

        var hiddenCaptionLabel = new Label
        {
            Text = "Hidden:",
            VerticalOptions = LayoutOptions.Center,
        };

        var hiddenCheckbox = new CheckBox
        {
            IsChecked = driveItem.IsHidden,
        };
        hiddenCheckbox.CheckedChanged += Hidden_CheckedChanged;

        grid.Add(consoleIpCaptionLabel, 0, 0);
        grid.Add(consoleIpLabel, 1, 0);
        grid.Add(itemNameCaptionLabel, 0, 1);
        grid.Add(itemNameLabel, 1, 1);
        grid.Add(itemTypeCaptionLabel, 0, 2);
        grid.Add(itemTypeLabel, 1, 2);
        grid.AddWithSpan(separatorLine1, 3, 0, 1, 2);
        grid.Add(locationCaptionLabel, 0, 4);
        grid.Add(locationLabel, 1, 4);
        grid.Add(sizeCaptionLabel, 0, 5);
        grid.Add(sizeLabel, 1, 5);
        grid.AddWithSpan(separatorLine2, 6, 0, 1, 2);
        grid.Add(createdCaptionLabel, 0, 7);
        grid.Add(createdLabel, 1, 7);
        grid.Add(modifiedCaptionLabel, 0, 8);
        grid.Add(modifiedLabel, 1, 8);
        grid.Add(readonlyCaptionLabel, 0, 9);
        grid.Add(readonlyCheckbox, 1, 9);
        grid.Add(hiddenCaptionLabel, 0, 10);
        grid.Add(hiddenCheckbox, 1, 10);

        var okButton = new Button
        {
            Text = "OK",
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalOptions = LayoutOptions.Start
        };
        okButton.Clicked += (sender, e) =>
        {
            Close();
        };

        grid.AddWithSpan(okButton, 11, 0, 1, 2);

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

        Content = border;
    }

    private async void Readonly_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        mDriveItem.Flags = e.Value ? (mDriveItem.Flags | DriveItemFlag.ReadOnly) : (mDriveItem.Flags & ~DriveItemFlag.ReadOnly);
        mNeedsUpdate.Invoke();
        var response = await SetFileAttributes.SendAsync(Globals.GlobalConnection, mDriveItem.CombinePath(), mDriveItem.Created, mDriveItem.Changed, (mDriveItem.Flags & DriveItemFlag.Hidden) == DriveItemFlag.Hidden, (mDriveItem.Flags & DriveItemFlag.ReadOnly) == DriveItemFlag.ReadOnly);
        if (RXDKXBDM.Utils.IsSuccess(response.ResponseCode) == false)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
        }
    }

    private async void Hidden_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        mDriveItem.Flags = e.Value ? (mDriveItem.Flags | DriveItemFlag.Hidden) : (mDriveItem.Flags & ~DriveItemFlag.Hidden);
        mNeedsUpdate.Invoke();
        var response = await SetFileAttributes.SendAsync(Globals.GlobalConnection, mDriveItem.CombinePath(), mDriveItem.Created, mDriveItem.Changed, (mDriveItem.Flags & DriveItemFlag.Hidden) == DriveItemFlag.Hidden, (mDriveItem.Flags & DriveItemFlag.ReadOnly) == DriveItemFlag.ReadOnly);
        if (RXDKXBDM.Utils.IsSuccess(response.ResponseCode) == false)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
        }
    }
}