using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Text;
using System.Threading.Tasks;
using FolderCompareApp.Models;

namespace FolderCompareApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _leftFolderPath;

        [ObservableProperty]
        private string _rightFolderPath;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isComparing;

        [ObservableProperty]
        private ObservableCollection<FileComparisonModel> _comparisonResults;

        [ObservableProperty]
        private ICollectionView _filteredComparisonResults;

        [ObservableProperty]
        private string _filterText;

        [ObservableProperty]
        private ComparisonStatus? _statusFilter;

        public MainViewModel()
        {
            ComparisonResults = new ObservableCollection<FileComparisonModel>();
            FilterText = string.Empty;
            StatusMessage = "Ready to compare folders";

            // Initialize the filtered view
            FilteredComparisonResults = CollectionViewSource.GetDefaultView(ComparisonResults);
            FilteredComparisonResults.Filter = FilterPredicate;
        }

        partial void OnFilterTextChanged(string value)
        {
            FilteredComparisonResults?.Refresh();
        }

        partial void OnStatusFilterChanged(ComparisonStatus? value)
        {
            FilteredComparisonResults?.Refresh();
        }

        private bool FilterPredicate(object item)
        {
            if (item is not FileComparisonModel file)
                return false;

            bool matchesText = string.IsNullOrWhiteSpace(FilterText) ||
                               file.Path.Contains(FilterText, StringComparison.OrdinalIgnoreCase);

            bool matchesStatus = StatusFilter == null || file.Status == StatusFilter;

            return matchesText && matchesStatus;
        }

        public string LeftFolderName => string.IsNullOrEmpty(LeftFolderPath) ? "Left Folder" : Path.GetFileName(LeftFolderPath);
        public string RightFolderName => string.IsNullOrEmpty(RightFolderPath) ? "Right Folder" : Path.GetFileName(RightFolderPath);

        private bool CanCompare()
        {
            return !string.IsNullOrEmpty(LeftFolderPath) &&
                   !string.IsNullOrEmpty(RightFolderPath) &&
                   Directory.Exists(LeftFolderPath) &&
                   Directory.Exists(RightFolderPath);
        }

        [RelayCommand(CanExecute = nameof(CanCompare))]
        private async Task CompareAsync()
        {
            IsComparing = true;
            StatusMessage = "Comparing folders...";
            ComparisonResults.Clear();

            try
            {
                await Task.Run(() =>
                {
                    // Get all files from both directories recursively
                    var leftFiles = GetFilesRecursively(LeftFolderPath);
                    var rightFiles = GetFilesRecursively(RightFolderPath);

                    // Create a set of relative paths for right files for faster lookup
                    var rightFileSet = new HashSet<string>(rightFiles.Keys);

                    // Compare files in left directory with right directory
                    foreach (var leftRelativePath in leftFiles.Keys)
                    {
                        var leftFileInfo = leftFiles[leftRelativePath];

                        if (rightFiles.TryGetValue(leftRelativePath, out FileInfo rightFileInfo))
                        {
                            // File exists in both directories, compare size and date
                            var sizeDifferent = leftFileInfo.Length != rightFileInfo.Length;
                            var dateDifferent = Math.Abs((leftFileInfo.LastWriteTime - rightFileInfo.LastWriteTime).TotalSeconds) > 1;

                            var result = new FileComparisonModel
                            {
                                Path = leftRelativePath,
                                Status = sizeDifferent || dateDifferent ? ComparisonStatus.Different : ComparisonStatus.Identical,
                                SizeLeft = leftFileInfo.Length,
                                SizeRight = rightFileInfo.Length,
                                DateLeft = leftFileInfo.LastWriteTime,
                                DateRight = rightFileInfo.LastWriteTime,
                                SizeDifferent = sizeDifferent,
                                DateDifferent = dateDifferent
                            };

                            Application.Current.Dispatcher.Invoke(() => ComparisonResults.Add(result));
                            rightFileSet.Remove(leftRelativePath);
                        }
                        else
                        {
                            // File only exists in left directory
                            Application.Current.Dispatcher.Invoke(() => ComparisonResults.Add(new FileComparisonModel
                            {
                                Path = leftRelativePath,
                                Status = ComparisonStatus.LeftOnly,
                                SizeLeft = leftFileInfo.Length,
                                SizeRight = 0,
                                DateLeft = leftFileInfo.LastWriteTime,
                                DateRight = DateTime.MinValue,
                                SizeDifferent = false,
                                DateDifferent = false
                            }));
                        }
                    }

                    // Add files that only exist in the right directory
                    foreach (var rightRelativePath in rightFileSet)
                    {
                        var rightFileInfo = rightFiles[rightRelativePath];

                        Application.Current.Dispatcher.Invoke(() => ComparisonResults.Add(new FileComparisonModel
                        {
                            Path = rightRelativePath,
                            Status = ComparisonStatus.RightOnly,
                            SizeLeft = 0,
                            SizeRight = rightFileInfo.Length,
                            DateLeft = DateTime.MinValue,
                            DateRight = rightFileInfo.LastWriteTime,
                            SizeDifferent = false,
                            DateDifferent = false
                        }));
                    }
                });

                // Update status message with summary
                int identical = ComparisonResults.Count(r => r.Status == ComparisonStatus.Identical);
                int different = ComparisonResults.Count(r => r.Status == ComparisonStatus.Different);
                int leftOnly = ComparisonResults.Count(r => r.Status == ComparisonStatus.LeftOnly);
                int rightOnly = ComparisonResults.Count(r => r.Status == ComparisonStatus.RightOnly);

                StatusMessage = $"Comparison complete. Found {identical} identical files, {different} different files, " +
                             $"{leftOnly} files only in left folder, {rightOnly} files only in right folder.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error comparing folders: {ex.Message}";
                await ShowErrorMessageAsync("Error", $"Error comparing folders: {ex.Message}");
            }
            finally
            {
                IsComparing = false;
            }
        }

        private Dictionary<string, FileInfo> GetFilesRecursively(string rootFolder)
        {
            var files = new Dictionary<string, FileInfo>();
            var rootDirInfo = new DirectoryInfo(rootFolder);

            foreach (var file in rootDirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                // Get the path relative to the root folder
                var relativePath = file.FullName.Substring(rootFolder.Length).TrimStart('\\', '/');
                files[relativePath] = file;
            }

            return files;
        }

        [RelayCommand]
        private void SelectLeftFolder()
        {
            var folderPath = ShowFolderDialog("Select Left Folder");
            if (!string.IsNullOrEmpty(folderPath))
            {
                LeftFolderPath = folderPath;
                StatusMessage = "Left folder selected: " + folderPath;
                CompareCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private void SelectRightFolder()
        {
            var folderPath = ShowFolderDialog("Select Right Folder");
            if (!string.IsNullOrEmpty(folderPath))
            {
                RightFolderPath = folderPath;
                StatusMessage = "Right folder selected: " + folderPath;
                CompareCommand.NotifyCanExecuteChanged();
            }
        }

        private string ShowFolderDialog(string title)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = title
            };

            return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
        }

        [RelayCommand]
        private async Task ExportResultsAsync()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                DefaultExt = "txt",
                Title = "Save Comparison Results"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    StatusMessage = "Exporting results...";

                    await Task.Run(() =>
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"Folder Comparison Results - {DateTime.Now}");
                        sb.AppendLine($"Left: {LeftFolderPath}");
                        sb.AppendLine($"Right: {RightFolderPath}");
                        sb.AppendLine(new string('-', 80));
                        sb.AppendLine($"{"Status",-15}{"Path",-50}{"Size L/R",-20}{"Date Modified Different",-10}");
                        sb.AppendLine(new string('-', 80));

                        foreach (var item in ComparisonResults)
                        {
                            sb.AppendLine($"{item.StatusText,-15}{(item.Path.Length > 50 ? "..." + item.Path.Substring(item.Path.Length - 47) : item.Path),-50}" +
                                        $"{item.SizeLeftFormatted}/{item.SizeRightFormatted},-20{(item.DateDifferent ? "Yes" : "No"),-10}");
                        }

                        File.WriteAllText(saveDialog.FileName, sb.ToString());
                    });

                    StatusMessage = $"Results exported to {saveDialog.FileName}";
                    await ShowInfoMessageAsync("Export Complete", $"Results exported to {saveDialog.FileName}");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error exporting results: {ex.Message}";
                    await ShowErrorMessageAsync("Error", $"Error exporting results: {ex.Message}");
                }
            }
        }

        // We'll use these methods to show messages, but in a real implementation
        // we would use a messaging service from the toolkit
        private Task ShowErrorMessageAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        private Task ShowInfoMessageAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }
    }
}