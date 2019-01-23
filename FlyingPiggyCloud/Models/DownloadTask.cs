﻿using FlyingPiggyCloud.Controllers.Results.FileSystem;
using Syroot.Windows.IO;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using FlyingPiggyCloud.Controllers;

namespace FlyingPiggyCloud.Models
{
    internal class DownloadTask : FlyingAria2c.DownloadTask, INotifyPropertyChanged,ICompletedTask
    {
        private readonly FileMetaData fileMetaData;

        public string FileName => fileMetaData.Name;

        public string Size => Calculators.SizeCalculator(fileMetaData.Size);

        public TaskTypeEnum TaskType => TaskTypeEnum.Download;

        public new async Task RefreshStatus()
        {
            await base.RefreshStatus();
            OnPropertyChanged("Status");
            OnPropertyChanged("Progress");
            OnPropertyChanged("DownloadSpeed");
            System.Threading.Thread.Sleep(500);
        }

        public DownloadTask(FileMetaData fileMetaData) : base(fileMetaData.DownloadAddress, async (e) =>
        {
            string NewFilePath = KnownFolders.Downloads.Path + "\\" + fileMetaData.Name;
            int index = 1;
            while (File.Exists(NewFilePath))
            {
                NewFilePath = Path.GetDirectoryName(KnownFolders.Downloads.Path + "\\" + fileMetaData.Name) + "\\" + Path.GetFileNameWithoutExtension(KnownFolders.Downloads.Path + "\\" + fileMetaData.Name) + string.Format("({0})",index) + Path.GetExtension(KnownFolders.Downloads.Path + "\\" + fileMetaData.Name);
                index++;
            }
            
            File.Move(await e.GetFilePath(), NewFilePath);
            e.FilePath = NewFilePath;
        })
        {
            this.fileMetaData = fileMetaData;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
