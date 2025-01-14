using System.Net;
using RXDKXBDM.Commands;
using RXDKNeighborhood.Controls;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Storage;
using RXDKXBDM.Models;

namespace RXDKNeighborhood;

[QueryProperty("IpAddress", "ipAddress")]
[QueryProperty("Path", "path")]
public partial class ConsolePage : ContentPage
{
    public string IpAddress { get; set; }

    public string Path { get; set; }

    public bool HasLoaded { get; set; }

    private DriveItem[] mDriveItems;

    public ConsolePage()
	{
        InitializeComponent();

        IpAddress = string.Empty;
        Path = string.Empty;
        HasLoaded = false;
        mDriveItems = [];
        SizeChanged += OnSizeChanged;
    }

    private void ShowBusy(string? caption = null)
    {
        BusyStatus.Text = caption ?? "Please wait...";
        BusyOverlay.IsVisible = true;
        BusyIndicator.IsRunning = true;
    }

    private void HideBusy()
    {
        BusyOverlay.IsVisible = false;
        BusyIndicator.IsRunning = false;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (HasLoaded == false)
        {
            ShowBusy();

            var connected = await ConnectToXboxAsync();
            if (connected == false)
            {
                await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                await Shell.Current.GoToAsync("..");
                HideBusy();
                return;
            }
            HasLoaded = true;
            HideBusy();
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        int columns = (int)(this.Width / 130);
        columns = Math.Max(1, columns);
        if (DriveCollectionView.ItemsLayout is GridItemsLayout gridItemsLayout)
        {
            gridItemsLayout.Span = columns;
        }
    }

    private string GetUtilityDriveTitle(IDictionary<string, string> utilDriveInfo, string key)
    {
        if (utilDriveInfo.TryGetValue(key, out string? value))
        {
            return $"Utility Drive for Title {value.Substring(2).ToUpper()}";
        }
        return "Utility Drive";
    }

    private string DriveLetterToName(string letter, IDictionary<string, string> utilDriveInfo)
    {
        var result = "Volume";

        if (letter == "C")
        {
            result = "Main Volume";
        }
        else if (letter == "D")
        {
            result = "Launch Volume";
        }
        else if (letter == "E")
        {
            result = "Game Development Volume";
        }
        else if (letter.CompareTo("F") >= 0 && letter.CompareTo("M") <= 0)
        {
            result = $"Memory Unit";
        }
        else if (letter == "P")
        {
            result = GetUtilityDriveTitle(utilDriveInfo, "Part2_LastTitleId");
        }
        else if (letter == "Q")
        {
            result = GetUtilityDriveTitle(utilDriveInfo, "Part1_TitleId");
        }
        else if (letter == "R")
        {
            result = GetUtilityDriveTitle(utilDriveInfo, "Part0_TitleId");
        }
        else if (letter == "S")
        {
            result = "Persistent Data - All Titles";
        }
        else if (letter == "T")
        {
            result = "Persistent Data - Active Title";
        }
        else if (letter == "U")
        {
            result = "Saved Games - Active Title";
        }
        else if (letter == "V")
        {
            result = "Saved Games - All Titles";
        }
        else if (letter == "X")
        {
            result = "Scratch Volume";
        }
        else if (letter == "Y")
        {
            result = "Xbox Dashboard Volume";
        }
        result = $"{result} ({letter}:)";
        return result;
    }

    public async Task<bool> ConnectToXboxAsync()
    {
        Globals.GlobalConnection.Close();

        if (IPAddress.TryParse(IpAddress, out _) == true)
        {
            if (await Globals.GlobalConnection.OpenAsync(IpAddress) == true)
            {
                if (Path == string.Empty)
                {
                    var utilDriveInfoResponse = await UtilDriveInfo.SendAsync(Globals.GlobalConnection);
                    if (RXDKXBDM.Utils.IsSuccess(utilDriveInfoResponse.ResponseCode) == false || utilDriveInfoResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    var driveListResponse = await DriveList.SendAsync(Globals.GlobalConnection);
                    if (RXDKXBDM.Utils.IsSuccess(driveListResponse.ResponseCode) == false || driveListResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    if (driveListResponse.ResponseValue != null)
                    {
                        var driveItems = new List<DriveItem>();
                        for (int i = 0; i < driveListResponse.ResponseValue.Length; i++)
                        {
                            var drive = driveListResponse.ResponseValue[i];
                            driveItems.Add(new DriveItem { Name = DriveLetterToName(drive, utilDriveInfoResponse.ResponseValue), Path = $"{drive}:", ImageUrl = "drive.png", Flags = DriveItemFlag.Drive });
                        }
                        mDriveItems = driveItems.ToArray();
                        PopulateDriveItems();
                    }
                }
                else
                {
                    var dirListResponse = await DirList.SendAsync(Globals.GlobalConnection, Path + "\\");
                    if (RXDKXBDM.Utils.IsSuccess(dirListResponse.ResponseCode) == false || dirListResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    mDriveItems = dirListResponse.ResponseValue;
                    PopulateDriveItems();
                }

                return true;
            }
        }
        return false;
    }

    private void PopulateDriveItems()
    {
        DriveCollectionView.ItemTemplate = new DataTemplate(() =>
        {
            var contentPresenter = new ContentView();
            contentPresenter.SetBinding(ContentView.ContentProperty, ".");
            return contentPresenter;
        });

        var driveItemViews = new List<View>();

        var sortedDriveItems = mDriveItems.OrderBy(d => d.IsFile).ThenBy(d => d.CombinePath()).ToArray();
        for (int i = 0; i < sortedDriveItems.Length; i++)
        {
            var driveItem = sortedDriveItems[i];

            var stackLayout = new TaggedStackLayout
            {
                Tag = driveItem,
                WidthRequest = 120,
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.Center
            };

            var image = new Image
            {
                WidthRequest = 100,
                HeightRequest = 100,
                HorizontalOptions = LayoutOptions.Center,
                Source = ImageSource.FromFile(driveItem.ImageUrl),
                Opacity = driveItem.IsHidden ? 0.25 : 1
            };
            stackLayout.Children.Add(image);

            var label = new Label
            {
                HeightRequest = 60,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 0, 10),
                Text = driveItem.Name
            };
            stackLayout.Children.Add(label);

            var tapGestureRecognizer = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 2
            };
            tapGestureRecognizer.Tapped += DriveItem_Tapped;
            stackLayout.GestureRecognizers.Add(tapGestureRecognizer);

            var menuFlyout = new MenuFlyout();

            var propertiesItem = new MenuFlyoutItem
            {
                Text = "Properties",
                CommandParameter = $"properties={driveItem.CombinePath()}",
            };
            propertiesItem.Clicked += MenuItem_Clicked;
            menuFlyout.Add(propertiesItem);
   
            if (driveItem.HasDownload)
            {
                var downloadItem = new MenuFlyoutItem
                {
                    Text = "Download",
                    CommandParameter = $"download={driveItem.CombinePath()}"
                };
                downloadItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(downloadItem);
            }
            if (driveItem.HasDelete)
            {
                var deleteItem = new MenuFlyoutItem
                {
                    Text = "Delete",
                    CommandParameter = $"delete={driveItem.CombinePath()}",
                    IsDestructive = true
                };
                deleteItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(deleteItem);
            }
            if (driveItem.HasLaunch)
            {
                var launchItem = new MenuFlyoutItem
                {
                    Text = "Launch",
                    CommandParameter = $"launch={driveItem.CombinePath()}"
                };
                launchItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(launchItem);
            }

            FlyoutBase.SetContextFlyout(stackLayout, menuFlyout);

            driveItemViews.Add(stackLayout);
        }

        DriveCollectionView.ItemsSource = driveItemViews;
    }

    private async void MenuItem_Clicked(object? sender, EventArgs e)
    {
        if (sender != null && sender is MenuFlyoutItem menuItem)
        {
            if (menuItem.BindingContext is TaggedStackLayout taggedStackLayout && taggedStackLayout.Tag is DriveItem driveItem && menuItem.CommandParameter is string commandParameter)
            {
                var index = commandParameter.IndexOf('=');
                if (index >= 0)
                {
                    var command = commandParameter.Substring(0, index);
                    var argument = commandParameter.Substring(index + 1);
                    if (command == "properties")
                    {
                        if (argument.EndsWith(':'))
                        {
                            var response = await DriveFreeSpace.SendAsync(Globals.GlobalConnection, argument);
                            if (RXDKXBDM.Utils.IsSuccess(response.ResponseCode) == false || response.ResponseValue == null)
                            {
                                await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                                return;
                            }

                            var totalBytes = RXDKXBDM.Utils.GetDictionaryLongFromKeys(response.ResponseValue, "totalbyteshi", "totalbyteslo");
                            var totalFreeBytes = RXDKXBDM.Utils.GetDictionaryLongFromKeys(response.ResponseValue, "totalfreebyteshi", "totalfreebyteslo");
                            var popup = new DriveProperriesPopup(driveItem, IpAddress, totalBytes, totalFreeBytes);
                            this.ShowPopup(popup);
                        }
                        else
                        {
                            var popup = new PathProperriesPopup(driveItem, IpAddress, () =>
                            {
                                PopulateDriveItems();
                            });
                            this.ShowPopup(popup);
                        }
                    }
                    else if (command == "launch")
                    {
                        var response = await MagicBoot.SendAsync(Globals.GlobalConnection, argument, true);
                        if (RXDKXBDM.Utils.IsSuccess(response) == false)
                        {
                            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                        }
                    }
                    else if (command == "delete")
                    {
                        await DisplayAlert("Error", "Delete not implemented yet.", "Ok");
                    }
                    else if (command == "download")
                    {
                        try
                        {
                            if (driveItem.IsDirectory)
                            {
                                var folder = await FolderPicker.Default.PickAsync();
                                if (folder.Folder == null)
                                {
                                    return;
                                }

                                ShowBusy();

                                var driveItems = await Utils.GetFolderComtents(driveItem, new CancellationToken());

                                HideBusy();

                                var popup = new DownloadPopup(driveItems, folder.Folder.Path);
                                this.ShowPopup(popup);

                                //bool success = await Utils.DownloadFolderAsync(argument, "", new CancellationToken(), (step, total) =>
                                //{
                                //    Dispatcher.Dispatch(() =>
                                //    {
                                //        BusyStatus.Text = $"Downloading {Utils.FormatBytes(step)} of {Utils.FormatBytes(total)}";
                                //    });
                                //});

                                //if (success == false)
                                //{
                                //    await DisplayAlert("Error", "Download failed.", "Ok");
                                //}
                            }
                            else
                            {
                                var filename = await Utils.FilePicker(Window, driveItem.Name);
                                if (filename == null)
                                {
                                    return;
                                }

                                ShowBusy();

                                bool success = await Utils.DownloadFileAsync(argument, filename, new CancellationToken(), (step, total) =>
                                {
                                    Dispatcher.Dispatch(() =>
                                    {
                                        BusyStatus.Text = $"Downloading {Utils.FormatBytes(step)} of {Utils.FormatBytes(total)}";
                                    });
                                });

                                if (success == false)
                                {
                                    await DisplayAlert("Error", "Download failed.", "Ok");
                                    if (File.Exists(filename))
                                    {
                                        File.Delete(filename);
                                    }
                                }

                                HideBusy();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            await DisplayAlert("Error", "Failed to save file.", "Ok");
                        }
                    }
                }
            }
        }
    }

    private async void DriveItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender != null && ((TaggedStackLayout)sender).Tag is DriveItem selectedDriveItem)
        {
            if (selectedDriveItem.IsDrive || selectedDriveItem.IsDirectory)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "ipAddress", IpAddress },
                    { "path", selectedDriveItem.CombinePath() }
                };
                Globals.GlobalConnection.Close();
                await Shell.Current.GoToAsync(nameof(ConsolePage), parameters);
            }
        }
    }

    private async void Warm_Clicked(object? sender, EventArgs e)
    {
        var response = await Reboot.SendAsync(Globals.GlobalConnection, true);
        if (RXDKXBDM.Utils.IsSuccess(response) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
            return;
        }
    }

    private async void WarmActiveTitle_Clicked(object? sender, EventArgs e)
    {
        var xbeInfoResponse = await XbeInfo.SendAsync(Globals.GlobalConnection, "");
        if (xbeInfoResponse.ResponseCode == ResponseCode.ERROR_NOSUCHFILE)
        {
            Warm_Clicked(null, new EventArgs());
        }
        else if (!RXDKXBDM.Utils.IsSuccess(xbeInfoResponse.ResponseCode))
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
            return;
        }

        if (xbeInfoResponse.ResponseValue == null || !xbeInfoResponse.ResponseValue.ContainsKey("name"))
        {
            await DisplayAlert("Error", "Unexpected response from Xbox.", "Ok");
            return;
        }

        var title = xbeInfoResponse.ResponseValue["name"];
        var magicBootResponse = await MagicBoot.SendAsync(Globals.GlobalConnection, title, true);
        if (RXDKXBDM.Utils.IsSuccess(magicBootResponse) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
        }
    }

    private async void Cold_Clicked(object? sender, EventArgs e)
    {
        var response = await Reboot.SendAsync(Globals.GlobalConnection, false);
        if (RXDKXBDM.Utils.IsSuccess(response) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
            return;
        }
    }

    private async void SyncronizeTime_Clicked(object? sender, EventArgs e)
    {
        var response = await SetSysTime.SendAsync(Globals.GlobalConnection, false);
        if (RXDKXBDM.Utils.IsSuccess(response.ResponseCode) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
            return;
        }
    }

    private async void Screenshot_Clicked(object? sender, EventArgs e)
    {
        var filename = await Utils.ImageFilePicker(Window, "screenshot.png");
        if (filename == null)
        {
            return;
        }

        ShowBusy();

        bool success = await Utils.DownloadScreenshotAsync(filename, new CancellationToken());
        if (success == false)
        {
            await DisplayAlert("Error", "Screenshot failed.", "Ok");
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
        else
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filename)
            });
        }

        HideBusy();
    }
}