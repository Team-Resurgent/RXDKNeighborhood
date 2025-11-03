using RXDKNeighborhood.Extensions;
using RXDKNeighborhood.Models;
using RXDKNeighborhood.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RXDKXBDM.Commands;
using ReactiveUI;
using RXDKXBDM.Models;
using Avalonia.Threading;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using RXDKXBDM;
using System.Threading;
using System.IO;
using RXDKNeighborhood.Helpers;

namespace RXDKNeighborhood.ViewModels
{
    public class MainWindowViewModel : ViewModelBase<MainWindow>
    {
        public Config Config { get; set; } = new Config();

        private string _currentPath = string.Empty;
        public string CurrentPath
        {
            get => _currentPath;
            set {
                var changed = _currentPath != value;
                this.RaiseAndSetIfChanged(ref _currentPath, value);
                if (changed)
                {
                    this.RaisePropertyChanged(nameof(CanReboot));
                    this.RaisePropertyChanged(nameof(CanDiscover));
                    this.RaisePropertyChanged(nameof(CanScreenshot));
                    this.RaisePropertyChanged(nameof(CanSynchronizeTime));
                    this.RaisePropertyChanged(nameof(CanDebug));
                    this.RaisePropertyChanged(nameof(CanCreateDirectory));
                    this.RaisePropertyChanged(nameof(CanUpload));
                    this.RaisePropertyChanged(nameof(CanRefresh));
                    this.RaisePropertyChanged(nameof(CanDragDrop));
                }
            }
        }

        public bool CanReboot => CurrentPath.Length > 0;

        public bool CanDiscover => CurrentPath.Length == 0;

        public bool CanScreenshot => CurrentPath.Length > 0;

        public bool CanSynchronizeTime => CurrentPath.Length > 0;

        public bool CanDebug => CurrentPath.Length > 0;

        public bool CanCreateDirectory => CurrentPath.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length > 1;

        public bool CanUpload => CurrentPath.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length > 1;

        public bool CanRefresh => CurrentPath.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length > 1;

        public bool CanDragDrop => CurrentPath.Split("\\", StringSplitOptions.RemoveEmptyEntries).Length > 1;

        public ObservableCollection<ConsoleItem> ConsoleItems { get; set; } = [];

        public ICommand ClearQueueCommand { get; }

        public ICommand RetryFailedCommand { get; }

        public TransferQueueProcessor TransferQueueProcessor { get; }

        public MainWindowViewModel()
        {
            PopulateConsoleItems();

            ClearQueueCommand = ReactiveCommand.Create(() =>
            {
                if (TransferQueueProcessor == null)
                {
                    return;
                }
                foreach (var item in TransferQueueProcessor.TransferDetails)
                {
                    item.CancellationTokenSource.Cancel();
                }
                TransferQueueProcessor.TransferDetails.Clear();
            });

            RetryFailedCommand = ReactiveCommand.Create(() =>
            {
                if (TransferQueueProcessor == null)
                {
                    return;
                }
                foreach (var item in TransferQueueProcessor.TransferDetails)
                {
                    if (!item.Failed)
                    {
                        continue;
                    }
                    item.Progress = "Pending";
                    item.Failed = false;
                }
            });

            TransferQueueProcessor = new TransferQueueProcessor();
            TransferQueueProcessor.TransferCompleted += TransferQueueProcessor_TransferCompleted;
            TransferQueueProcessor.Start();
        }

        private void TransferQueueProcessor_TransferCompleted()
        {
            PopulateConsoleItems();
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
            return result;
        }

        private async void PopulateConsoleItems()
        {
            ConsoleItems.Clear();

            await Task.Run(async () =>
            {
                if (!Config.TryLoadConfig(out var config) || config == null)
                {
                    config = new Config();
                }

                var parts = CurrentPath.Split("\\", StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                {
                    Config = config;

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        ConsoleItems.Add(new() { Name = "Add Xbox", Description = null, Value = null, Image = new Uri("/Images/add_xbox.png", UriKind.Relative).ToBitmap(), Type = ConsoleItemType.AddXbox });

                        var sortedXboxItems = Config.XboxItemList.OrderBy(c => c.Name).ToArray();
                        for (int i = 0; i < sortedXboxItems.Length; i++)
                        {
                            var xboxItem = sortedXboxItems[i];
                            var consoleItem = new ConsoleItem { Name = xboxItem.Name, Description = xboxItem.IpAddress, Value = xboxItem, Image = new Uri("/Images/xbox.png", UriKind.Relative).ToBitmap(), Type = ConsoleItemType.Xbox };
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                ConsoleItems.Add(consoleItem);
                            });
                        }
                    });
                }
                else if (parts.Length == 1)
                {
                    using var connection = new Connection();
                    if (await connection.OpenAsync(parts[0]))
                    {
                        var utilDriveInfoResponse = await GetUtilDriveInfo.SendAsync(connection);
                        var driveListResponse = await DriveList.SendAsync(connection);
                        if (Utils.IsSuccess(driveListResponse.ResponseCode) == false || driveListResponse.ResponseValue == null)
                        {
                            return;
                        }

                        if (driveListResponse.ResponseValue != null)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                var consoleItems = new List<ConsoleItem>();
                                var sortedDriveItems = driveListResponse.ResponseValue.OrderBy(c => c.Name).ToArray();
                                for (int i = 0; i < sortedDriveItems.Length; i++)
                                {
                                    var driveItem = sortedDriveItems[i];
                                    var consoleItem = new ConsoleItem { Name = $"{DriveLetterToName(driveItem.Value, utilDriveInfoResponse.ResponseValue)} ({driveItem.Value})", Description = driveItem.Name, Value = driveItem, Image = new Uri("/Images/drive.png", UriKind.Relative).ToBitmap(), Type = ConsoleItemType.Drive };

                                    ConsoleItems.Add(consoleItem);
                                }
                            });
                        }
                    }
                }
                else
                {
                    using var connection = new Connection();
                    if (await connection.OpenAsync(parts[0]))
                    {
                        var path = parts[1] + ":";
                        for (var i = 2; i < parts.Length; i++)
                        {
                            path += "\\" + parts[i];
                        }
                        path += "\\";
                        var dirListResponse = await DirList.SendAsync(connection, path);
                        if (Utils.IsSuccess(dirListResponse.ResponseCode) == false || dirListResponse.ResponseValue == null)
                        {
                            return;
                        }

                        Dispatcher.UIThread.Invoke(() =>
                        {
                            var consoleItems = new List<ConsoleItem>();
                            var sortedFileSystemItems = dirListResponse.ResponseValue.OrderBy(c => !c.IsDirectory).ThenBy(c => c.Name).ToArray();
                            for (int i = 0; i < dirListResponse.ResponseValue.Length; i++)
                            {
                                var fileSystemItem = sortedFileSystemItems[i];
                                var image = new Uri(fileSystemItem.IsFile ? "/Images/file.png" : "/Images/directory.png", UriKind.Relative).ToBitmap();
                                var consoleItem = new ConsoleItem { Name = fileSystemItem.Name, Description = "", Value = fileSystemItem, Image = image, Type = ConsoleItemType.FileSystem };
                                ConsoleItems.Add(consoleItem);
                            }
                        });
                    }
                }
            });
        }

        private async Task ShowAlert(string title, string prompt)
        {
            if (Owner == null)
            {
                return;
            }
            var alertDialogWindow = new AlertDialogWindow();
            var alertDialogWindowViewModel = new AlertDialogWindowViewModel { Owner = alertDialogWindow, Title = title, Prompt = prompt };
            alertDialogWindow.DataContext = alertDialogWindowViewModel;
            await alertDialogWindow.ShowDialog(Owner);
        }

        private async Task ShowInput(string title, string prompt, string input, Action<string?> closingAction)
        {
            if (Owner == null)
            {
                return;
            }
            var inputDialogWindow = new InputDialogWindow();
            var inputDialogWindowViewModel = new InputDialogWindowViewModel { Owner = inputDialogWindow, Title = title, Prompt = prompt, Input = input };
            inputDialogWindow.DataContext = inputDialogWindowViewModel;
            inputDialogWindowViewModel.OnClosing += closingAction;
            await inputDialogWindow.ShowDialog(Owner);
        }

        private async Task ShowConfirm(string title, string prompt, Action<bool> closingAction)
        {
            if (Owner == null)
            {
                return;
            }
            var confirmDialogWindow = new ConfirmDialogWindow();
            var confirmDialogWindowViewModel = new ConfirmDialogWindowViewModel { Owner = confirmDialogWindow, Title = title, Prompt = prompt };
            confirmDialogWindow.DataContext = confirmDialogWindowViewModel;
            confirmDialogWindowViewModel.OnClosing += closingAction;
            await confirmDialogWindow.ShowDialog(Owner);
        }

        public async void ItemClicked(ConsoleItem item)
        {
            if (Owner == null)
            {
                return;
            }
            if (item.Type == ConsoleItemType.AddXbox)
            {
                await ShowInput("Add Xbox", "Xbox IP address:", string.Empty, async (s) =>
                {
                    if (s == null)
                    {
                        return;
                    }

                    if (IPAddress.TryParse(s, out _) == true)
                    {
                        using var connection = new Connection();
                        if (await connection.OpenAsync(s) == true)
                        {
                            var response = await DbgName.SendAsync(connection);
                            if (Utils.IsSuccess(response.ResponseCode) == false)
                            {
                                await ShowAlert("Error", "Failed to connect to Xbox.");
                                return;
                            }
                            if (!Config.XboxItemList.Any(x => x.IpAddress == response.ResponseValue))
                            {
                                Config.XboxItemList.Add(new XboxItem(response.ResponseValue, s));
                                Config.TrySaveConfig(Config);
                                PopulateConsoleItems();
                            }
                            return;
                        }

                        await ShowAlert("Error", "Failed to connect to Xbox.");
                        return;
                    }
                    await ShowAlert("Error", "Invalid IP address specified.");
                });
                return;
            }

            if (item.Type == ConsoleItemType.Xbox && item.Value is XboxItem xboxItemValue && xboxItemValue?.IpAddress != null)
            {
                CurrentPath = xboxItemValue.IpAddress;
                PopulateConsoleItems();
            }

            if (item.Type == ConsoleItemType.Drive && item.Value is DriveItem driveItemValue && driveItemValue.Value != null)
            {
                CurrentPath = CurrentPath + "\\" + driveItemValue.Value;
                PopulateConsoleItems();
            }

            if (item.Type == ConsoleItemType.FileSystem && item.Value is FileSystemItem fileSystemItemValue && fileSystemItemValue?.IsDirectory == true)
            {
                CurrentPath = CurrentPath + "\\" + fileSystemItemValue.Name;
                PopulateConsoleItems();
            }
        }

        public void Back()
        {
            CurrentPath = CurrentPath.ParentXboxPath();
            PopulateConsoleItems();
        }

        public void Discover()
        {
            var xboxItems = XboxDiscovery.Discover();
            foreach (var xboxItem in xboxItems)
            {
                if (Config.XboxItemList.Any(x => x.IpAddress == xboxItem.IpAddress))
                {
                    continue;
                }
                Config.XboxItemList.Add(xboxItem);
            }
            Config.TrySaveConfig(Config);
            PopulateConsoleItems();
        }

        public void RemoveXbox(ConsoleItem item)
        {
            if (item.Value is not XboxItem xboxItem)
            {
                return;
            }
            Config.XboxItemList.Remove(xboxItem);
            Config.TrySaveConfig(Config);
            PopulateConsoleItems();
        }

        public async void WarmReboot(string ipAddress)
        {
            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var response = await Reboot.SendAsync(connection, true, false, WaitType.None);
                if (Utils.IsSuccess(response.ResponseCode) == false)
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                }
            }
        }

        public async void WarmRebootTitle(string ipAddress)
        {
            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var xbeInfoResponse = await XbeInfo.SendAsync(connection, "");
                if (xbeInfoResponse.ResponseCode == ResponseCode.ERROR_NOSUCHFILE)
                {
                    WarmReboot(ipAddress);
                }
                else if (!Utils.IsSuccess(xbeInfoResponse.ResponseCode))
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                    return;
                }

                if (xbeInfoResponse?.ResponseValue?.ContainsKey("name") != true)
                {
                    await ShowAlert("Error", "Unexpected response from Xbox.");
                    return;
                }

                var title = xbeInfoResponse.ResponseValue["name"];
                var magicBootResponse = await MagicBoot.SendAsync(connection, title, true);
                if (Utils.IsSuccess(magicBootResponse.ResponseCode) == false)
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                }
            }
        }

        public async void ColdReboot(string ipAddress)
        {
            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var response = await Reboot.SendAsync(connection, false, false, WaitType.None);
                if (Utils.IsSuccess(response.ResponseCode) == false)
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                }
            }
        }

        public async void Screenshot(string ipAddress)
        {
            if (Owner == null)
            {
                return;
            }

            var options = new FilePickerSaveOptions
            {
                Title = "Save Image As",
                SuggestedFileName = "image.png",
                FileTypeChoices =
                [
                    new("PNG Image")
                    {
                        Patterns = ["*.png"],
                        MimeTypes = ["image/png"]
                    }
                ],
                DefaultExtension = "png"
            };

            var file = await Owner.StorageProvider.SaveFilePickerAsync(options);
            if (file == null)
            {
                return;
            }

            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                bool success = await Utils.DownloadScreenshotAsync(connection, file.Path.LocalPath, new CancellationToken());
                if (success == false)
                {
                    await ShowAlert("Error", "Screenshot failed.");
                }
            };
        }

        public async void SynchronizeTime(string ipAddress)
        {
            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var response = await SetSysTime.SendAsync(connection, false);
                if (Utils.IsSuccess(response.ResponseCode) == false)
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                }
            }
        }

        public void Debug(string ipAddress)
        {
            if (Owner == null)
            {
                return;
            }
            var debugWindow = new DebugWindow();
            var debugWindowViewModel = new DebugWindowViewModel { Owner = debugWindow, IpAddress = ipAddress };
            debugWindow.DataContext = debugWindowViewModel;
            debugWindow.Show();
        }

        public async void Download(ConsoleItem item)
        {
            if (Owner == null || item.Value is not FileSystemItem fileSystemItem)
            {
                return;
            }

            try
            {
                var options = new FolderPickerOpenOptions
                {
                    Title = "Select a folder",
                    AllowMultiple = false,
                };

                var folder = await Owner.StorageProvider.OpenFolderPickerAsync(options);
                if (folder == null || folder.Count == 0)
                {
                    return;
                }

                CurrentPath.FormatXboxPath(out var ipAddress, out var _);

                var folderPath = folder[0].Path.LocalPath;
                var transferDetail = new TransferDetail
                {
                    IpAddress = ipAddress,
                    TransferType = TransferType.Download,
                    Failed = false,
                    Progress = "Pending",
                    SourcePath = Path.Combine(fileSystemItem.Path, fileSystemItem.Name),
                    DestPath = Path.Combine(folderPath, fileSystemItem.Name),
                    IsDirectory = fileSystemItem.IsDirectory,
                    FileSize = fileSystemItem.Size
                };
                TransferQueueProcessor.TransferDetails.Add(transferDetail);
            }
            catch 
            {
                // do nothing
            }
        }

        public async void UploadFile()
        {
            if (Owner == null)
            {
                return;
            }

            try
            {
                var options = new FilePickerOpenOptions
                {
                    Title = "Open File",
                    AllowMultiple = true,
                    FileTypeFilter = [ new FilePickerFileType("All files") { Patterns = ["*.*"] } ]
                };

                var files = await Owner.StorageProvider.OpenFilePickerAsync(options);
                if (files == null)
                {
                    return;
                }

                CurrentPath.FormatXboxPath(out var ipAddress, out var path);
                foreach (var file in files)
                {
                    UploadFilePath(file.Path.LocalPath);
                }
            }
            catch
            {
                // do nothing
            }
        }

        public void UploadFilePath(string filePath)
        {
            CurrentPath.FormatXboxPath(out var ipAddress, out var path);
            var transferDetail = new TransferDetail
            {
                IpAddress = ipAddress,
                TransferType = TransferType.Upload,
                Failed = false,
                Progress = "Pending",
                SourcePath = filePath,
                DestPath = Path.Combine(path, Path.GetFileName(filePath)),
                IsDirectory = false,
                FileSize = (ulong)new FileInfo(filePath).Length,
            };
            TransferQueueProcessor.TransferDetails.Add(transferDetail);
        }

        public async void UploadFolder()
        {
            if (Owner == null)
            {
                return;
            }

            try
            {
                var options = new FolderPickerOpenOptions
                {
                    Title = "Select a folder",
                    AllowMultiple = true,
                };

                var folders = await Owner.StorageProvider.OpenFolderPickerAsync(options);
                if (folders == null)
                {
                    return;
                }

                CurrentPath.FormatXboxPath(out var ipAddress, out var path);
                foreach (var folder in folders)
                {
                    UploadFolderPath(folder.Path.LocalPath);
                }
            }
            catch
            {
                // do nothing
            }
        }

        public void UploadFolderPath(string folderPath)
        {
            folderPath = folderPath.TrimEnd('\\');

            CurrentPath.FormatXboxPath(out var ipAddress, out var path);
            var transferDetail = new TransferDetail
            {
                IpAddress = ipAddress,
                TransferType = TransferType.Upload,
                Failed = false,
                Progress = "Pending",
                SourcePath = folderPath,
                DestPath = Path.Combine(path, Path.GetFileName(folderPath)),
                IsDirectory = true,
                FileSize = 0,
            };
            TransferQueueProcessor.TransferDetails.Add(transferDetail);
        }

        public void Refresh()
        {
            PopulateConsoleItems();
        }

        public async void Launch(ConsoleItem item)
        {
            if (item.Value is not FileSystemItem fileSystemItem)
            {
                return;
            }
            using var connection = new Connection();
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var response = await MagicBoot.SendAsync(connection, Path.Combine(fileSystemItem.Path, fileSystemItem.Name), true);
                if (Utils.IsSuccess(response.ResponseCode) == false)
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                }
            }
        }

        public void LaunchWithDebug(ConsoleItem item)
        {
            if (item.Value is not FileSystemItem fileSystemItem)
            {
                return;
            }
            if (Owner == null)
            {
                return;
            }
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            var xbePath = Path.Combine(fileSystemItem.Path, fileSystemItem.Name);
            var debugWindow = new DebugWindow();
            var debugWindowViewModel = new DebugWindowViewModel { Owner = debugWindow, IpAddress = ipAddress, XbePath = xbePath };
            debugWindow.DataContext = debugWindowViewModel;
            debugWindow.Show();
        }

        public async void CreateDirectory()
        {
            await ShowInput("Create Directory", "Enter name:", string.Empty, async (s) =>
            {
                if (string.IsNullOrEmpty(s)) // + validate fatx
                {
                    return;
                }
                using var connection = new Connection();
                CurrentPath.FormatXboxPath(out var ipAddress, out var path);
                if (await connection.OpenAsync(ipAddress) == true)
                {
                    var response = await MkDir.SendAsync(connection, Path.Combine(path, s));
                    if (Utils.IsSuccess(response.ResponseCode) == false)
                    {
                        await ShowAlert("Error", "Failed to connect to Xbox.");
                        return;
                    }
                }
                PopulateConsoleItems();
            });
        }

        public async void Rename(ConsoleItem item)
        {
            if (item.Value is not FileSystemItem fileSystemItem)
            {
                return;
            }

            await ShowInput("Rename", "Enter new name:", fileSystemItem.Name, async (s) =>
            {
                if (string.IsNullOrEmpty(s)) // + validate fatx
                {
                    return;
                }
                using var connection = new Connection();
                CurrentPath.FormatXboxPath(out var ipAddress, out var _);
                if (await connection.OpenAsync(ipAddress) == true)
                {
                    var response = await RXDKXBDM.Commands.Rename.SendAsync(connection, Path.Combine(fileSystemItem.Path, fileSystemItem.Name), Path.Combine(fileSystemItem.Path, s));
                    if (Utils.IsSuccess(response.ResponseCode) == false)
                    {
                        await ShowAlert("Error", "Failed to connect to Xbox.");
                        return;
                    }
                }
                PopulateConsoleItems();
            });
  
        }

        private async Task<bool> DeleteFileSystemItem(FileSystemItem fileSystemItem)
        {
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var path = Path.Combine(fileSystemItem.Path, fileSystemItem.Name);
                var response = await RXDKXBDM.Commands.Delete.SendAsync(connection, path, fileSystemItem.IsDirectory);
                if (Utils.IsSuccess(response.ResponseCode) == false)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private async Task<FileSystemItem[]?> GetDirectoryListing(FileSystemItem fileSystemItem)
        {
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            using var connection = new Connection();
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var dirListResponse = await DirList.SendAsync(connection, Path.Combine(fileSystemItem.Path, fileSystemItem.Name));
                if (!Utils.IsSuccess(dirListResponse.ResponseCode) || dirListResponse.ResponseValue == null)
                {
                    return null;
                }
                return dirListResponse.ResponseValue;
            }
            return null;
        }

        public async void Delete(ConsoleItem item)
        {
            if (item.Value is not FileSystemItem fileSystemItem)
            {
                return;
            }

            await ShowConfirm("Delete", "Are you sure?", async (s) =>
            {
                if (s == false)
                {
                    return;
                }

                CurrentPath.FormatXboxPath(out var ipAddress, out var _);

                if (fileSystemItem.IsFile)
                {
                    if (await DeleteFileSystemItem(fileSystemItem) == false)
                    {
                        await ShowAlert("Error", "Failed to connect to Xbox.");
                        return;
                    }
                }
                else
                {
                    var foldersToDelete = new List<FileSystemItem>();
                    var pendingFolders = new Stack<FileSystemItem>();
                    pendingFolders.Push(fileSystemItem);

                    while (pendingFolders.Count > 0)
                    {
                        var currentFolder = pendingFolders.Pop();
                        foldersToDelete.Add(currentFolder);

                        var dirListResponse = await GetDirectoryListing(currentFolder);
                        if (dirListResponse == null)
                        {
                            await ShowAlert("Error", "Failed to connect to Xbox.");
                            return;
                        }

                        foreach (var currentItem in dirListResponse)
                        {
                            if (currentItem.IsFile)
                            {
                                if (await DeleteFileSystemItem(currentItem) == false)
                                {
                                    await ShowAlert("Error", "Failed to connect to Xbox.");
                                    return;
                                }
                            }
                            else
                            {
                                pendingFolders.Push(currentItem);
                            }
                        }
                    }

                    foldersToDelete.Sort((a, b) =>
                    {
                        var pathA = Path.Combine(a.Path, a.Name);
                        var pathB = Path.Combine(b.Path, b.Name);
                        return pathB.Count(c => c == Path.DirectorySeparatorChar).CompareTo(pathA.Count(c => c == Path.DirectorySeparatorChar));
                    });

                    foreach (var folder in foldersToDelete)
                    {
                        if (await DeleteFileSystemItem(folder) == false)
                        {
                            await ShowAlert("Error", "Failed to connect to Xbox.");
                            return;
                        }
                    }
                }

                PopulateConsoleItems();
            });
        }

        public void ShowProperties(ConsoleItem item)
        {
            if (Owner == null)
            {
                return;
            }
            if (item.Value is DriveItem driveItem)
            {
                ShowDriveProperties(Owner, driveItem);
            }
            else if (item.Value is FileSystemItem fileSystemItem)
            {
                if (fileSystemItem.IsDirectory)
                {
                    ShowDirectoryProperties(Owner, fileSystemItem);
                }
                else
                {
                    ShowFileProperties(Owner, fileSystemItem);
                }
            }
        }

        public async void ShowFileProperties(MainWindow owner, FileSystemItem fileSystemItem)
        {
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            var filePropertiesWindow = new FilePropertiesWindow();
            var filePropertiesWindowViewModel = new FilePropertiesWindowViewModel
            {
                Owner = filePropertiesWindow,
                FileSystemItem = fileSystemItem,
                IpAddress = ipAddress,
            };
            filePropertiesWindowViewModel.OnClosing += (changed) =>
            {
                if (!changed)
                {
                    return;
                }
                PopulateConsoleItems();
            };
            filePropertiesWindow.DataContext = filePropertiesWindowViewModel;
            await filePropertiesWindow.ShowDialog(owner);
        }

        public async void ShowDirectoryProperties(MainWindow owner, FileSystemItem fileSystemItem)
        {
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            var directoryPropertiesWindow = new DirectoryPropertiesWindow();
            var directoryPropertiesWindowViewModel = new DirectoryPropertiesWindowViewModel
            {
                Owner = directoryPropertiesWindow,
                FileSystemItem = fileSystemItem,
                IpAddress = ipAddress,
                Size = "0 bytes",
                Contains = "0 Files, 0 Folders",
            };
            directoryPropertiesWindowViewModel.TriggerUpdate();
            directoryPropertiesWindowViewModel.OnClosing += (changed) =>
            {
                if (!changed)
                {
                    return;
                }
                PopulateConsoleItems();
            };
            directoryPropertiesWindow.DataContext = directoryPropertiesWindowViewModel;
            await directoryPropertiesWindow.ShowDialog(owner);
        }

        public async void ShowDriveProperties(MainWindow owner, DriveItem driveItem)
        {
            if (Owner == null)
            {
                return;
            }

            using var connection = new Connection();
            CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            if (await connection.OpenAsync(ipAddress) == true)
            {
                var utilDriveInfoResponse = await GetUtilDriveInfo.SendAsync(connection);
                var response = await DriveFreeSpace.SendAsync(connection, driveItem.Name);
                if (Utils.IsSuccess(response.ResponseCode) == false || response.ResponseValue == null)
                {
                    await ShowAlert("Error", "Failed to connect to Xbox.");
                    return;
                }

                var totalBytes = Utils.GetDictionaryLongFromKeys(response.ResponseValue, "totalbyteshi", "totalbyteslo");
                var totalFreeBytes = Utils.GetDictionaryLongFromKeys(response.ResponseValue, "totalfreebyteshi", "totalfreebyteslo");
                var drivePropertiesWindow = new DrivePropertiesWindow();
                var drivePropertiesWindowViewModel = new DrivePropertiesWindowViewModel
                {
                    Owner = drivePropertiesWindow,
                    Title = $"{driveItem.Value} on {ipAddress} Properties",
                    Drive = $"{driveItem.Value} on {ipAddress}",
                    Type = DriveLetterToName(driveItem.Value, utilDriveInfoResponse.ResponseValue),
                    UsedSpaceBytes = totalBytes - totalFreeBytes,
                    UsedSpaceBytesFormatted = (totalBytes - totalFreeBytes).ToString("N0") + " bytes",
                    UsedSpaceFormatted = StringExtension.FormatBytes(totalBytes - totalFreeBytes),
                    FreeSpaceBytes = totalFreeBytes,
                    FreeSpaceBytesFormatted = totalFreeBytes.ToString("N0") + " bytes",
                    FreeSpaceFormatted = StringExtension.FormatBytes(totalFreeBytes),
                    CapacitySpaceBytesFormatted = totalBytes.ToString("N0") + " bytes",
                    CapacitySpaceFormatted = StringExtension.FormatBytes(totalBytes)
                };

                drivePropertiesWindow.DataContext = drivePropertiesWindowViewModel;
                await drivePropertiesWindow.ShowDialog(owner);
            }
        }

    }
}
