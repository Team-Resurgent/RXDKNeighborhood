using System.Net;
using RXDKXBDM.Commands;
using RXDKNeighborhood.ViewModels;
using RXDKNeighborhood.Controls;

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

    protected override async void OnNavigatedTo(NavigatedToEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (HasLoaded == false)
        {
            var connected = await ConnectToXboxAsync();
            if (connected == false)
            {
                await DisplayAlert("Error", "Failed to connect to Xbox.", "Ok");
                await Shell.Current.GoToAsync("..");
                return;
            }
            HasLoaded = true;
            BusyOverlay.IsVisible = false;
            BusyIndicator.IsRunning = false;
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

    public ulong HexWordsToLong(string hiValue, string loValue)
    {
        var hi = Convert.ToUInt32(hiValue, 16);
        var lo = Convert.ToUInt32(loValue, 16);
        var result = ((ulong)hi << 32) | (ulong)lo;
        return result;
    }

    public string GetDictionaryString(IDictionary<string, string> keyValues, string key)
    {
        var result = keyValues.TryGetValue(key, out string? value) ? value : "";
        return result;
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
                    if (utilDriveInfoResponse.IsSuccess() == false || utilDriveInfoResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    var driveListResponse = await DriveList.SendAsync(Globals.GlobalConnection);
                    if (driveListResponse.IsSuccess() == false || driveListResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    if (driveListResponse.ResponseValue != null)
                    {
                        for (int i = 0; i < driveListResponse.ResponseValue.Length; i++)
                        {
                            var drive = driveListResponse.ResponseValue[i];
                            driveItems.Add(new DriveItem { Name = DriveLetterToName(drive, utilDriveInfoResponse.ResponseValue), Path = $"{drive}:", ImageUrl = "drive.png", Type = DriveItemType.Drive });
                        }
                    }
                }
                else
                {
                    var dirListResponse = await DirList.SendAsync(Globals.GlobalConnection, Path);
                    if (dirListResponse.IsSuccess() == false || dirListResponse.ResponseValue == null)
                    {
                        Globals.GlobalConnection.Close();
                        return false;
                    }

                    for (var i = 0; i < dirListResponse.ResponseValue.Length; i++)
                    {
                        var itemProperties = dirListResponse.ResponseValue[i];

                        var tempName = GetDictionaryString(itemProperties, "name");
                        var name = tempName.Substring(1, tempName.Length - 2);
                        var path = $"{Path}\\{name}";

                        var sizehi = GetDictionaryString(itemProperties, "sizehi");
                        var sizelo = GetDictionaryString(itemProperties, "sizelo");
                        var size = HexWordsToLong(sizehi, sizelo);

                        var createhi = GetDictionaryString(itemProperties, "createhi");
                        var createlo = GetDictionaryString(itemProperties, "createlo");
                        var create = DateTime.FromFileTime((long)HexWordsToLong(createhi, createlo));

                        var changehi = GetDictionaryString(itemProperties, "changehi");
                        var changelo = GetDictionaryString(itemProperties, "changelo");
                        var change = DateTime.FromFileTime((long)HexWordsToLong(changehi, changelo));

                        var type = itemProperties.ContainsKey("directory") ? DriveItemType.Directory : DriveItemType.File;
                        var imageUrl = itemProperties.ContainsKey("directory") ? "directory.png" : "file.png";

                        var driveItem = new DriveItem { Name = name, Path = path, Size = size, Created = create, Changed = change, ImageUrl = imageUrl, Type = type };
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

        for (int i = 0; i < driveItems.Length; i++)
        {
            var driveItem = driveItems[i];

            var stackLayout = new TaggedStackLayout
            {
                Tag = driveItem,
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
                    CommandParameter = $"properties={driveItem.Path}",
                };
                propertiesItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(propertiesItem);
            }
            if (driveItem.HasDownload)
            {
                var downloadItem = new MenuFlyoutItem
                {
                    Text = "Download",
                    CommandParameter = $"download={driveItem.Path}"
                };
                downloadItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(downloadItem);
            }
            if (driveItem.HasDelete)
            {
                var deleteItem = new MenuFlyoutItem
                {
                    Text = "Delete",
                    CommandParameter = $"delete={driveItem.Path}",
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
                    CommandParameter = $"launch={driveItem.Path}"
                };
                launchItem.Clicked += MenuItem_Clicked;
                menuFlyout.Add(launchItem);
            }

            FlyoutBase.SetContextFlyout(stackLayout, menuFlyout);

            DriveItemsTest.Add(stackLayout);
        }

        DriveCollectionView.ItemsSource = DriveItemsTest;
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
                    if (command == "properties")
                    {

                    }
                    else if (command == "launch")
                    {

                    }
                    else if (command == "delete")
                    {

                    }
                    else if (command == "launch")
                    {

                    }
                }
            }
            
        }
    }

    private async void DriveItem_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender != null && ((TaggedStackLayout)sender).Tag is DriveItem selectedDriveItem)
        {
            if (selectedDriveItem.Type == DriveItemType.Drive || selectedDriveItem.Type == DriveItemType.Directory)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "ipAddress", IpAddress },
                    { "path", selectedDriveItem.Path }
                };
                Globals.GlobalConnection.Close();
                await Shell.Current.GoToAsync(nameof(ConsolePage), parameters);
            }
        }
    }

    private async void QuickBoot_Clicked(object sender, EventArgs e)
    {
        _ = await Reboot.SendAsync(Globals.GlobalConnection, true);
    }

    private async void Reboot_Clicked(object sender, EventArgs e)
    {
        _ = await Reboot.SendAsync(Globals.GlobalConnection, false);
    }
}