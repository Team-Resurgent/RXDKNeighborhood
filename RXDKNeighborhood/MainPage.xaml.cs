using RXDKNeighborhood.ViewModels;
using RXDKNeighborhood.Models;
using System.Collections.ObjectModel;
using RXDKXBDM;
using RXDKXBDM.Commands;
using System.Net;
using RXDKNeighborhood.Controls;
using Microsoft.Maui.Graphics.Text;

namespace RXDKNeighborhood
{
    public partial class MainPage : ContentPage
    {
        public Config mConfig;

        public MainPage()
        {
            InitializeComponent();

            mConfig = new Config();
            Config.TryLoadConfig(ref mConfig);
            PopulateConsoleItems([.. mConfig.ConsoleDetailList]);

            SizeChanged += OnSizeChanged;
        }

        private void PopulateConsoleItems(ConsoleDetail[] consoleDetails)
        {
            var consoleItems = new List<ConsoleItem>
            {
                new() { Name = "Add Xbox", Description = "", ImageUrl = "add_xbox.png", Type = ConsoleItemType.AddXbox }
            };
            for (int i = 0; i < consoleDetails.Length; i++)
            {
                var consoleDetail = consoleDetails[i];
                consoleItems.Add(new() { Name = consoleDetail.Name, Description = consoleDetail.IpAddress, ImageUrl = "xbox.png", Type = ConsoleItemType.XboxOriginal });
            }

            XboxCollectionView.ItemTemplate = new DataTemplate(() =>
            {
                var contentPresenter = new ContentView();
                contentPresenter.SetBinding(ContentView.ContentProperty, ".");
                return contentPresenter;
            });

            var consoleItemViews = new List<View>();
            for (int i = 0; i < consoleItems.Count; i++)
            {
                var consoleItem = consoleItems[i];

                var stackLayout = new TaggedStackLayout
                {
                    Tag = consoleItem,
                    WidthRequest = 120,
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.Center
                };

                var image = new Image
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                    HorizontalOptions = LayoutOptions.Center,
                    Source = ImageSource.FromFile(consoleItem.ImageUrl)
                };
                stackLayout.Children.Add(image);

                var labelName = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold,
                    Text = consoleItem.Name
                };
                stackLayout.Children.Add(labelName);

                var labelDescription = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextColor = Colors.Gray,
                    Text = consoleItem.Description
                };
                stackLayout.Children.Add(labelDescription);

                var tapGestureRecognizer = new TapGestureRecognizer
                {
                    NumberOfTapsRequired = 2
                };
                tapGestureRecognizer.Tapped += ConsoleItem_Tapped;
                stackLayout.GestureRecognizers.Add(tapGestureRecognizer);

                if (consoleItem.HasDelete)
                {
                    var menuFlyout = new MenuFlyout();
                    var propertiesItem = new MenuFlyoutItem
                    {
                        Text = "Delete",
                        CommandParameter = $"delete={consoleItem.Description}",
                        IsDestructive = true
                    };
                    propertiesItem.Clicked += MenuItem_Clicked;
                    menuFlyout.Add(propertiesItem);

                    FlyoutBase.SetContextFlyout(stackLayout, menuFlyout);
                }

                consoleItemViews.Add(stackLayout);
            }

            XboxCollectionView.ItemsSource = consoleItemViews;
        }

        private void OnSizeChanged(object? sender, EventArgs e)
        {
            int columns = (int)(this.Width / 130);
            columns = Math.Max(1, columns);
            if (XboxCollectionView.ItemsLayout is GridItemsLayout gridItemsLayout)
            {
                gridItemsLayout.Span = columns;
            }
        }

        private async Task ShowInputDialogAsync()
        {
            var input = await Shell.Current.DisplayPromptAsync("Add Xbox", "Xbox IP address:");
            if (input == null)
            {
                return;
            }

            if (IPAddress.TryParse(input, out _) == true)
            {
                var connection = new Connection();
                if (await connection.OpenAsync(input) == true)
                {
                    var response = await DebugName.SendAsync(connection);
                    if (response.IsSuccess() == false)
                    {
                        await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                        connection.Close();
                        return;
                    }

                    mConfig.ConsoleDetailList.Add(new ConsoleDetail(response.ResponseValue, input));
                    Config.TrySaveConfig(mConfig);
                    PopulateConsoleItems([.. mConfig.ConsoleDetailList]);
                    connection.Close();
                    return;
                }

                await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                return;
            }

            await DisplayAlert("Error", "Invalid IP address specified.", "Ok");
        }

        private void MenuItem_Clicked(object? sender, EventArgs e)
        {
            if (sender != null && sender is MenuFlyoutItem menuItem)
            {
                if (menuItem.CommandParameter is string commandParameter)
                {
                    var index = commandParameter.IndexOf("=");
                    if (index >= 0)
                    {
                        var command = commandParameter.Substring(0, index);
                        var argument = commandParameter.Substring(index + 1);
                        if (command == "delete")
                        {
                            for (int i = 0; i < mConfig.ConsoleDetailList.Count; i++)
                            {
                                var consoleDetail = mConfig.ConsoleDetailList[i];
                                if (consoleDetail.IpAddress == argument)
                                {
                                    mConfig.ConsoleDetailList.Remove(consoleDetail);
                                    Config.TrySaveConfig(mConfig);
                                    PopulateConsoleItems([.. mConfig.ConsoleDetailList]);
                                }
                            }
                        }
                    }
                }

            }
        }

        private async void ConsoleItem_Tapped(object? sender, TappedEventArgs e)
        {
            if (sender != null && ((TaggedStackLayout)sender).Tag is ConsoleItem selectedConsoleItem)
            {
                if (selectedConsoleItem.Type == ConsoleItemType.AddXbox)
                {
                    await ShowInputDialogAsync();
                }
                else
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "ipAddress", selectedConsoleItem.Description },
                        { "path", string.Empty }
                    };
                    await Shell.Current.GoToAsync(nameof(ConsolePage), parameters);
                }
            }
        }
    }
}
