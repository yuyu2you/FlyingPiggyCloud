﻿using FlyingPiggyCloud.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wangsu.WcsLib.Core;

namespace FlyingPiggyCloud.Views
{
    /// <summary>
    /// FilesListPage.xaml 的交互逻辑
    /// </summary>
    public partial class FilesListPage : Page
    {
        private FileList fileList;

        private List<string> pathArray;

        /// <summary>
        /// 为后退按钮保存历史路径
        /// </summary>
        private Stack<string> previousPath;

        /// <summary>
        /// 为前进按钮保存历史路径
        /// </summary>
        private Stack<string> nextPath;

        /// <summary>
        /// 通过指定路径创建文件列表页，如果路径不存在将自动创建
        /// </summary>
        /// <param name="path"></param>
        public FilesListPage(string path)
        {
            fileList = new FileList("", path, true);
            InitializeComponent();
            FileListView.ItemsSource = fileList;
            CreatePathArray(path);
            previousPath = new Stack<string>();
            nextPath = new Stack<string>();
        }

        private void CreatePathArray(string path)
        {
            if (path == "/")
                pathArray = new List<string>(new string[]
                {
                    "root"
                });
            else
                pathArray = new List<string>(System.Text.RegularExpressions.Regex.Split(path, "/", System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace));
            AddressBar.ItemsSource = pathArray;
        }

        private async void ListViewItem_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            FileListItem a = (FileListItem)((ListViewItem)sender).DataContext;
            if (a.Type == 1)
            {
                if (a.IsEnable)
                {
                    previousPath.Push(fileList.CurrentPath);
                    nextPath.Clear();
                    await fileList.GetDirectoryByUUID(((FileListItem)((ListViewItem)sender).DataContext).UUID);
                    CreatePathArray(a.Path);
                }
                else
                {
                    MessageBox.Show("这个项目被远程服务器锁定（可能的原因：正在被删除，或者其他客户端正在修改该项目）");
                    fileList.Refresh(this, new EventArgs());
                }
            }
            
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string NewFolderName = "新文件夹";
            await fileList.NewFolder(NewFolderName, true);
            fileList.Refresh(sender, e);
        }

        private async void DownloadBotton_Click(object sender, RoutedEventArgs e)
        {
            if (((FileListItem)((Button)sender).DataContext).Type == 0)
            {
                string uuid = ((FileListItem)((Button)sender).DataContext).UUID;
                Controllers.FileSystemMethods fileSystemMethods = new Controllers.FileSystemMethods(Properties.Settings.Default.BaseUri);
                Controllers.Results.ResponesResult<Controllers.Results.FileSystem.FileMetaData> x = await fileSystemMethods.GetDetailsByUUID(uuid);
                DownloadingListPage.NewDownloadTask(x.Result);
            }
        }

        private void MoreBotton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            btn.ContextMenu.DataContext = btn.DataContext;
            btn.ContextMenu.IsOpen = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            MenuItem x = (MenuItem)sender;
            //x.IsEnabled = false;
            await ((FileListItem)x.DataContext).Remove();
            //System.Threading.Thread.Sleep(200);
            lock (fileList)
            {
                fileList.Refresh(this, new System.EventArgs());
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                UploadingListPage.NewUploadTask(openFileDialog, fileList.CurrentUUID);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Keyboard.ClearFocus();
            var btn = (Button)sender;
            var tbx = (TextBox)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(btn), 0);
            tbx.Text = ((FileListItem)tbx.DataContext).Name;
            btn.Visibility = Visibility.Collapsed;
            ((Button)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(btn), 1)).Visibility = Visibility.Collapsed;

        }

        private void Name_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            ((Button)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent((DependencyObject)sender), 1)).Visibility = Visibility.Visible;
            ((Button)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent((DependencyObject)sender), 2)).Visibility = Visibility.Visible;
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Keyboard.ClearFocus();
            var btn = (Button)sender;
            var tbx = (TextBox)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(btn), 0);
            await ((FileListItem)tbx.DataContext).Rename(tbx.Text);
            btn.Visibility = Visibility.Collapsed;
            ((Button)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent(btn), 2)).Visibility = Visibility.Collapsed;
        }

        private void Name_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            //失去键盘焦点后延迟250ms隐藏按钮，应当有更好的解决办法
            Task.Run(()=>
            {
                System.Threading.Thread.Sleep(250);
                App.Current.Dispatcher.Invoke(() =>
                {
                    ((Button)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent((DependencyObject)sender), 1)).Visibility = Visibility.Collapsed;
                    ((Button)VisualTreeHelper.GetChild(VisualTreeHelper.GetParent((DependencyObject)sender), 2)).Visibility = Visibility.Collapsed;
                });
            });
        }

        private async void AddressBar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ((ListBox)sender).SelectedIndex;
            if(index!=-1)
            {
                ((ListBox)sender).SelectedIndex = -1;
                string path = "";
                if (pathArray[0] == "root")
                    path = "/";
                else
                {
                    for (int i = 1; i <= index; i++)
                    {
                        path = path + "/" + pathArray[i];
                    }
                }
                previousPath.Push(fileList.CurrentPath);
                nextPath.Clear();
                await fileList.GetDirectoryByPath(path);
                CreatePathArray(path);
            }
        }

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            nextPath.Push(fileList.CurrentPath);
            if(previousPath.Count>0)
            {
                bool success;
                do
                {
                    try
                    {
                        string path = previousPath.Pop();
                        await fileList.GetDirectoryByPath(path);
                        success = true;
                        CreatePathArray(path);
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        success = false;
                    }
                } while (!success && previousPath.Count != 0);
            }
            
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            previousPath.Push(fileList.CurrentPath);
            if(nextPath.Count>0)
            {
                bool success;
                do
                {
                    try
                    {
                        string path = nextPath.Pop();
                        await fileList.GetDirectoryByPath(path);
                        success = true;
                        CreatePathArray(path);
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        success = false;
                    }
                } while (!success && nextPath.Count != 0);
            }
        }
    }
}
