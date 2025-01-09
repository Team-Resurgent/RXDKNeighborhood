using System.Net;
using RXDKXBDM.Commands;
using RXDKNeighborhood.ViewModels;
using RXDKNeighborhood.Controls;
using RXDKXBDM;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Storage;
using Windows.Storage.Pickers;
using Microsoft.Maui;
using System.Diagnostics;

namespace RXDKNeighborhood;

[QueryProperty("IpAddress", "ipAddress")]
[QueryProperty("Path", "path")]
public partial class ConsolePage : ContentPage
{
    public string IpAddress { get; set; }

    public string Path { get; set; }

    public bool HasLoaded { get; set; }

    public ConsolePage()
	{
        InitializeComponent();

        IpAddress = string.Empty;
        Path = string.Empty;
        HasLoaded = false;
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
                var driveItems = new List<DriveItem>();

                if (Path == string.Empty)
                {
                    var utilDriveInfoResponse = await UtilDriveInfo.SendAsync(Globals.GlobalConnection);
                    if (Utils.IsSuccess(utilDriveInfoResponse.ResponseCode) == false || utilDriveInfoResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    var driveListResponse = await DriveList.SendAsync(Globals.GlobalConnection);
                    if (Utils.IsSuccess(driveListResponse.ResponseCode) == false || driveListResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    if (driveListResponse.ResponseValue != null)
                    {
                        for (int i = 0; i < driveListResponse.ResponseValue.Length; i++)
                        {
                            var drive = driveListResponse.ResponseValue[i];
                            driveItems.Add(new DriveItem { Name = DriveLetterToName(drive, utilDriveInfoResponse.ResponseValue), Path = $"{drive}:", ImageUrl = "drive.png", Flags = DriveItemFlag.Drive });
                        }
                    }
                }
                else
                {
                    var dirListResponse = await DirList.SendAsync(Globals.GlobalConnection, Path);
                    if (Utils.IsSuccess(dirListResponse.ResponseCode) == false || dirListResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    for (var i = 0; i < dirListResponse.ResponseValue.Length; i++)
                    {
                        var itemProperties = dirListResponse.ResponseValue[i];

                        var name = Utils.GetDictionaryString(itemProperties, "name");
                        var path = $"{Path}\\";
                        var size = Utils.GetDictionaryLongFromKeys(itemProperties, "sizehi", "sizelo");
                        var create = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "createhi", "createlo"));
                        var change = DateTime.FromFileTime((long)Utils.GetDictionaryLongFromKeys(itemProperties, "changehi", "changelo"));
                        var imageUrl = itemProperties.ContainsKey("directory") ? "directory.png" : "file.png";

                        var flags = itemProperties.ContainsKey("directory") ? DriveItemFlag.Directory : DriveItemFlag.File;
                        if (itemProperties.ContainsKey("readonly"))
                        {
                            flags |= DriveItemFlag.ReadOnly;
                        }
                        if (itemProperties.ContainsKey("hidden"))
                        {
                            flags |= DriveItemFlag.Hidden;
                        }

                        var driveItem = new DriveItem { Name = name, Path = path, Size = size, Created = create, Changed = change, ImageUrl = imageUrl,  Flags = flags };
                        driveItems.Add(driveItem);
                    }
                }

                PopulateDriveItems(driveItems.ToArray());
                return true;
            }
        }
        return false;
    }

    private void PopulateDriveItems(DriveItem[] driveItems)
    {
        DriveCollectionView.ItemTemplate = new DataTemplate(() =>
        {
            var contentPresenter = new ContentView();
            contentPresenter.SetBinding(ContentView.ContentProperty, ".");
            return contentPresenter;
        });

        var DriveItemsTest = new List<View>();

        var sortedDriveItems = driveItems.OrderBy(d => (d.Flags & DriveItemFlag.File)).ThenBy(d => d.CombinePath()).ToArray();
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
                Source = ImageSource.FromFile(driveItem.ImageUrl)
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

            if (driveItem.HasProerties)
            {
                var propertiesItem = new MenuFlyoutItem
                {
                    Text = "Properties",
                    CommandParameter = $"properties={driveItem.CombinePath()}",
                };
                propertiesItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(propertiesItem);
            }
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

            DriveItemsTest.Add(stackLayout);
        }

        DriveCollectionView.ItemsSource = DriveItemsTest;
    }

    private async void MenuItem_Clicked(object? sender, EventArgs e)
    {
        if (sender != null && sender is MenuFlyoutItem menuItem)
        {
            if (menuItem.BindingContext is TaggedStackLayout taggedStackLayout && taggedStackLayout.Tag is DriveItem driveItem && menuItem.CommandParameter is string commandParameter)
            {

                var index = commandParameter.IndexOf("=");
                if (index >= 0)
                {
                    var command = commandParameter.Substring(0, index);
                    var argument = commandParameter.Substring(index + 1);
                    if (command == "properties")
                    {
                        if (argument.EndsWith(":"))
                        {
                            var response = await DriveFreeSpace.SendAsync(Globals.GlobalConnection, argument);
                            if (Utils.IsSuccess(response.ResponseCode) == false || response.ResponseValue == null)
                            {
                                await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                                return;
                            }

                            var totalBytes = Utils.GetDictionaryLongFromKeys(response.ResponseValue, "totalbyteshi", "totalbyteslo");
                            var totalFreeBytes = Utils.GetDictionaryLongFromKeys(response.ResponseValue, "totalfreebyteshi", "totalfreebyteslo");
                            var popup = new DriveProperriesPopup(driveItem, IpAddress, totalBytes, totalFreeBytes);
                            this.ShowPopup(popup);
                        }
                        else
                        {
                            var popup = new PathProperriesPopup(driveItem, IpAddress);
                            this.ShowPopup(popup);
                        }
                    }
                    else if (command == "launch")
                    {
                        var response = await MagicBoot.SendAsync(Globals.GlobalConnection, argument, true);
                        if (Utils.IsSuccess(response) == false)
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


                            if ((driveItem.Flags & DriveItemFlag.Directory) == DriveItemFlag.Directory)
                            {
                                await DisplayAlert("Error", "Directory not implemented yet.", "Ok");
                                return;
                            }

                            var filename = await FileUtils.FilePicker(Window, driveItem.Name);
                            if (filename == null)
                            {
                                return;
                            }

                            ShowBusy();

                            bool errorOccured = await Task.Run(() =>
                            {
                                bool errorOccured = false;
                                using (var fileStream = new FileStream(filename, FileMode.Create))
                                using (var downloadStream = new DownloadStream(fileStream))
                                {
                                    var timer = new System.Timers.Timer(100);
                                    timer.Elapsed += (sender, e) =>
                                    {
                                        Dispatcher.Dispatch(() =>
                                        {
                                            BusyStatus.Text = $"Downloading {downloadStream.Position} of {downloadStream.ExpectedSize}";
                                        });
                                    };
                                    timer.AutoReset = true;
                                    timer.Start();

                                    var response = Download.SendAsync(Globals.GlobalConnection, argument, downloadStream).Result;

                                    timer.Stop();

                                    if (downloadStream.ExpectedSize != downloadStream.Length)
                                    {
                                        errorOccured = true;
                                        Dispatcher.Dispatch(async () =>
                                        {
                                            await DisplayAlert("Error", "File saved does not match expected size.", "Ok");
                                        });
                                    }

                                    if (Utils.IsSuccess(response.ResponseCode) == false)
                                    {
                                        errorOccured = true;
                                        Dispatcher.Dispatch(async () =>
                                        {
                                            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                                        });
                                    }
                                }
                                return errorOccured;
                            });

                            if (errorOccured == true)
                            {
                                if (File.Exists(filename))
                                {
                                    File.Delete(filename);
                                }
                            }

                            HideBusy();

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
            if ((selectedDriveItem.Flags & DriveItemFlag.Drive) == DriveItemFlag.Drive || (selectedDriveItem.Flags & DriveItemFlag.Directory) == DriveItemFlag.Directory)
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
        if (Utils.IsSuccess(response) == false)
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
        else if (!Utils.IsSuccess(xbeInfoResponse.ResponseCode))
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
        if (Utils.IsSuccess(magicBootResponse) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
        }
    }

    private async void Cold_Clicked(object? sender, EventArgs e)
    {
        var response = await Reboot.SendAsync(Globals.GlobalConnection, false);
        if (Utils.IsSuccess(response) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
            return;
        }
    }

    private async void SyncronizeTime_Clicked(object? sender, EventArgs e)
    {
        var response = await SetSysTime.SendAsync(Globals.GlobalConnection, false);
        if (Utils.IsSuccess(response.ResponseCode) == false)
        {
            await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
            return;
        }
    }

    private void Screenshot_Clicked(object? sender, EventArgs e)
    {
        //using var outputStream = new FileStream("C:\\download.xbe", FileMode.CreateNew);
        //var response = await RXDKXBDM.Commands.Screenshot.SendAsync(Globals.GlobalConnection, outputStream);
        //if (Utils.IsSuccess(response.ResponseCode) == false)
        //{
        //    await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
        //    return;
        //}
    }
}