using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using RXDKNeighborhood.Controls;
using RXDKXBDM.Models;

namespace RXDKNeighborhood;

public class DriveProperriesPopup : Popup
{
    public DriveProperriesPopup(DriveItem driveItem, string ipAddress, ulong totalBytes, ulong totalFreeBytes)
    {
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
                new RowDefinition { Height = GridLength.Auto }
            }
        };

        var usedSpaceColor = Colors.Blue;
        var freeSpaceColor = Colors.Purple;

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

        var driveCaptionLabel = new Label
        {
            Text = "Drive:",
            VerticalOptions = LayoutOptions.Center,
        };

        var driveNameLabel = new Label
        {
            Text = driveItem.CombinePath(),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var usedSpaceCaptionLabel = new Label
        {
            Text = "Used space:",
            TextColor = usedSpaceColor,
            VerticalOptions = LayoutOptions.Center,
        };

        var usedSpaceSizeLabel = new Label
        {
            Text = (totalBytes - totalFreeBytes).ToString("N0") + " bytes",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var freeSpaceCaptionLabel = new Label
        {
            Text = "Free space:",
            TextColor = freeSpaceColor,
            VerticalOptions = LayoutOptions.Center,
        };

        var freeSpaceSizeLabel = new Label
        {
            Text = totalFreeBytes.ToString("N0") + " bytes",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var separatorLine = new BoxView
        {
            Color = Colors.Gray, 
            Margin = new Thickness(0, 5, 0, 5),
            HeightRequest = 1, 
            HorizontalOptions = LayoutOptions.Fill
        };

        var capacityCaptionLabel = new Label
        {
            Text = "Capacity:",
            VerticalOptions = LayoutOptions.Center,
        };

        var capacitySizeLabel = new Label
        {
            Text = totalBytes.ToString("N0") + " bytes",
            HorizontalTextAlignment = TextAlignment.End,
            VerticalOptions = LayoutOptions.Center,
        };

        var pieChart = new GraphicsView
        {
            Drawable = new DriveSpacePieDrawable(totalFreeBytes, freeSpaceColor, totalBytes - totalFreeBytes, usedSpaceColor),
            HeightRequest = 200,
            WidthRequest = 200
        };

        grid.Add(consoleIpCaptionLabel, 0, 0);
        grid.Add(consoleIpLabel, 1, 0);
        grid.Add(driveCaptionLabel, 0, 1);
        grid.Add(driveNameLabel, 1, 1);
        grid.Add(usedSpaceCaptionLabel, 0, 2);
        grid.Add(usedSpaceSizeLabel, 1, 2);
        grid.Add(freeSpaceCaptionLabel, 0, 3);
        grid.Add(freeSpaceSizeLabel, 1, 3);
        grid.AddWithSpan(separatorLine, 4, 0, 1, 2);
        grid.Add(capacityCaptionLabel, 0, 5);
        grid.Add(capacitySizeLabel, 1, 5);

        grid.AddWithSpan(pieChart, 6, 0, 1, 2);

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

        grid.AddWithSpan(okButton, 7, 0, 1, 2);

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
}