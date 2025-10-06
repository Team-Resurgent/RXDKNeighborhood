using Avalonia.Controls;
using RXDKNeighborhood.ViewModels;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.IO;
using System;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Threading;
using RXDKXBDM.Models;
using RXDKNeighborhood.Extensions;

namespace RXDKNeighborhood.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

      
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                if (vm.CanDragDrop && e.DataTransfer.Contains(DataFormat.File) == true)
                {
                    e.DragEffects = DragDropEffects.Copy;
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            }
            e.Handled = true;
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                if (vm.CanDragDrop && e.DataTransfer.Contains(DataFormat.File) == true)
                {
                    e.DragEffects = DragDropEffects.Copy;
                }
                else
                {
                    e.DragEffects = DragDropEffects.None;
                }
            }
            e.Handled = true;
        }

        private void OnDrop(object? sender, DragEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            if (e.DataTransfer.Contains(DataFormat.File) == true)
            {
                var items = e.DataTransfer.GetItems(DataFormat.File);
                foreach (var item in items)
                {
                    var file = item.TryGetFile();
                    if (file == null)
                    {
                        continue;
                    }
                    if (Directory.Exists(file.Path.LocalPath))
                    {
                        vm.UploadFolderPath(file.Path.LocalPath);
                    }
                    else if (File.Exists(file.Path.LocalPath))
                    {
                        vm.UploadFilePath(file.Path.LocalPath);
                    }
                }
            }
        }

        private void Border_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as Border).Properties.IsLeftButtonPressed)
            {
                if (e.ClickCount == 2)
                {
                    if (sender is Border border && border.DataContext is ConsoleItem item && DataContext is MainWindowViewModel vm)
                    {
                        vm.ItemClicked(item);
                    }
                }
            }
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Back();
        }

        private void Discover_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Discover();
        }

        private void RemoveXbox_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.RemoveXbox(item);
        }

        private void WarmReboot_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            vm.WarmReboot(ipAddress);
        }

        private void WarmRebootTitle_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            vm.WarmRebootTitle(ipAddress);
        }

        private void ColdReboot_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            vm.ColdReboot(ipAddress);
        }

        private void Screenshot_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            vm.Screenshot(ipAddress);
        }

        private void SynchronizeTime_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.CurrentPath.FormatXboxPath(out var ipAddress, out var _);
            vm.SynchronizeTime(ipAddress);
        }

        private void WarmRebootXbox_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm || item.Value is not XboxItem xboxItem)
            {
                return;
            }
            vm.WarmReboot(xboxItem.IpAddress);
        }

        private void WarmRebootTitleXbox_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm || item.Value is not XboxItem xboxItem)
            {
                return;
            }
            vm.WarmRebootTitle(xboxItem.IpAddress);
        }

        private void ColdRebootXbox_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm || item.Value is not XboxItem xboxItem)
            {
                return;
            }
            vm.ColdReboot(xboxItem.IpAddress);
        }

        private void ScreenshotXbox_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm || item.Value is not XboxItem xboxItem)
            {
                return;
            }
            vm.Screenshot(xboxItem.IpAddress);
        }

        private void SynchronizeTimeXbox_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm || item.Value is not XboxItem xboxItem)
            {
                return;
            }
            vm.SynchronizeTime(xboxItem.IpAddress);
        }

        private void Download_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Download(item);
        }

        private void Launch_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Launch(item);
        }

        private void Rename_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Rename(item);
        }

        private void CreateDirectory_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.CreateDirectory();
        }

        private void UploadFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.UploadFile();
        }

        private void UploadFolder_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.UploadFolder();
        }

        private void Refresh_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Refresh();
        }

        private void Delete_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.Delete(item);
        }

        private void ShowProperties_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.DataContext is not ConsoleItem item || DataContext is not MainWindowViewModel vm)
            {
                return;
            }
            vm.ShowProperties(item);
        }
    }
}