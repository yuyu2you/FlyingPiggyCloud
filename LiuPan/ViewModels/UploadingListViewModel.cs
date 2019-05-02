﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SixCloud.ViewModels
{
    internal class UploadingListViewModel : ViewModelBase
    {
        public ObservableCollection<UploadingTaskViewModel> ObservableCollection => _observableCollection;
        private static readonly ObservableCollection<UploadingTaskViewModel> _observableCollection = new ObservableCollection<UploadingTaskViewModel>();

        public static async Task NewTask(FileListViewModel targetList, string path)
        {
            await NewTask(targetList.CurrentUUID, path);
        }

        public static async Task NewTask(string targetUUID, string path)
        {
            if (Directory.Exists(path))
            {

            }
            else if (File.Exists(path))
            {
                UploadingFileViewModel task = await Task.Run(() => new UploadingFileViewModel(targetUUID, path));
                _observableCollection.Add(task);
                task.UploadCompleted += CompletedEventHandler;
                void CompletedEventHandler(object sender, EventArgs e)
                {
                    task.UploadCompleted -= CompletedEventHandler;
                    _observableCollection.Remove(task);
                    UploadedListViewModel.NewTask(task);
                };
                task.UploadAborted += AbortedEventHandler;
                void AbortedEventHandler(object sender, EventArgs e)
                {
                    task.UploadAborted -= AbortedEventHandler;
                    _observableCollection.Remove(task);
                };
            }
            else
            {
                MessageBox.Show("由于找不到对象，6盘未能创建任务", "失败", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

    }
}