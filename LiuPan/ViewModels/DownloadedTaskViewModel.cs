﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SixCloud.ViewModels
{
    internal class DownloadedTaskViewModel : ViewModelBase
    {
        private readonly string fullPath;

        public string Name { get; private set; }

        public DateTime CompletedTime { get; private set; }

        public ICommand OpenCommand { get; private set; }
        private void Open(object parameter)
        {
            try
            {
                Process.Start(fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "打开文件失败");
                Deleted?.Invoke(this, new EventArgs());
            }
        }

        public ICommand ShowCommand { get; private set; }
        private void Show(object parameter)
        {
            if(!File.Exists(fullPath))
            {
                MessageBox.Show("找不到文件，可能已被删除", "打开文件失败");
                Deleted?.Invoke(this, new EventArgs());
                return;
            }
            ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe")
            {
                Arguments = "/e,/select," + fullPath
            };
            Process.Start(psi);
        }

        public ICommand DeleteCommand { get; private set; }
        private void Delete(object parameter)
        {
            try
            {
                File.Delete(fullPath);
                Deleted?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "删除失败");
            }
        }
        public event EventHandler Deleted;

        public DownloadedTaskViewModel(DownloadingTaskViewModel task)
        {
            fullPath = task.CurrentFileFullPath;
            Name = task.Name;
            CompletedTime = DateTime.Now;
            OpenCommand = new DependencyCommand(Open, DependencyCommand.AlwaysCan);
            ShowCommand = new DependencyCommand(Show, DependencyCommand.AlwaysCan);
            DeleteCommand = new DependencyCommand(Delete, DependencyCommand.AlwaysCan);
        }
    }

}
