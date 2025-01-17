﻿using SixCloud.Controllers;
using SixCloud.Models;
using SixCloud.Views;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace SixCloud.ViewModels
{
    internal class FileListItemViewModel : FileSystemViewModel
    {
        public string Name { get; set; }

        public string MTime { get; set; }

        public string ATime { get; set; }

        public string CTime { get; set; }

        public bool Locking { get; set; }

        public bool Preview { get; set; }

        public int PreviewType { get; set; }

        ///// <summary>
        ///// 0为文件，1为目录
        ///// </summary>
        //public int Type { get; set; }

        public bool Directory { get; set; }

        public string Size { get; set; }

        public string Icon { get; set; }

        private static readonly Dictionary<string, string> IconDictionary;

        public string UUID { get; set; }
        public string Mime { get; private set; }

        public string Path { get; private set; }

        private bool AlwaysCan(object parameter)
        {
            return true;
        }

        private readonly FileListViewModel Parent;

        #region Copy
        public DependencyCommand CopyCommand { get; private set; }

        internal async void NewPreView()
        {
#warning 这里的代码还没有完成
            //if (Preview == 300)
            //{
            //    GenericResult<PreviewImageInformation> x = await Task.Run(() =>
            //    {
            //        return fileSystem.ImagePreview(UUID);
            //    });

            //    PreView preView = new PreView(PreView.ResourceType.Picture, x.Result.Url, x.Result,x.Token);
            //    preView.Show();
            //}
            if (PreviewType == 3010)
            {
                GenericResult<PreviewVideoInformation> x = await Task.Run(() =>
                {
                    return fileSystem.VideoPreview(UUID);
                });
                if (x.Success)
                {
                    PreView preView = new PreView(PreView.ResourceType.Video, x.Result.PreviewHlsAddress, x.Result);
                    preView.Show();
                }
            }
        }

        private void Copy(object parameter)
        {
            FileListViewModel.CopyList = new string[]
            {
                    UUID
            };
            FileListViewModel.CutList = null;
            Parent.StickCommand.OnCanExecutedChanged(this, new EventArgs());
        }
        #endregion

        #region Cut
        public DependencyCommand CutCommand { get; private set; }

        private void Cut(object parameter)
        {
            FileListViewModel.CutList = new string[]
            {
                    UUID
            };
            FileListViewModel.CopyList = null;
            Parent.StickCommand.OnCanExecutedChanged(this, new EventArgs());
        }
        #endregion

        #region Delete
        public AsyncCommand DeleteCommand { get; private set; }

        private void Delete(object parameter)
        {
            fileSystem.Remove(UUID);
        }
        #endregion

        #region Rename
        public DependencyCommand RenameCommand { get; private set; }

        private bool _IsRename = false;

        private void Rename(object parameter)
        {
            _IsRename = true;
            ConfirmCommand.OnCanExecutedChanged(this, new EventArgs());
        }
        #endregion

        #region Confirm
        public AsyncCommand ConfirmCommand { get; private set; }

        private void Confirm(object parameter)
        {
            if (parameter is string newName)
            {
                fileSystem.Rename(UUID, newName);
            }
        }

        private bool CanConfirm(object parameter)
        {
            return _IsRename;
        }
        #endregion

        #region Download
        public DependencyCommand DownloadCommand { get; private set; }

        private void Download(object parameter)
        {
            System.Windows.Forms.FolderBrowserDialog downloadPathDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "请选择下载文件夹",
                SelectedPath = KnownFolders.Downloads.Path
            };
            if (downloadPathDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Task.Run(() =>
                {
                    GenericResult<FileMetaData> x = fileSystem.GetDetailsByUUID(UUID);
                    if (!string.IsNullOrWhiteSpace(x.Result.DownloadAddress))
                    {
                        DownloadingListViewModel.NewTask(x.Result.DownloadAddress, downloadPathDialog.SelectedPath, Name);
                    }
                });

            }
        }

        private bool CanDownload(object parameter)
        {
            return !Directory;
        }
        #endregion

        #region MoreCommand
        public DependencyCommand MoreCommand { get; private set; }

        private void More(object parameter)
        {
            if (parameter is ButtonBase btn)
            {
                btn.ContextMenu.DataContext = btn.DataContext;
                btn.ContextMenu.IsOpen = true;
            }
        }
        #endregion

        static FileListItemViewModel()
        {
            IconDictionary = new Dictionary<string, string>
            {
                {"default",'\uf15c'.ToString() },
                {"folder",'\uf07b'.ToString() },
                {".zip",'\uf1c6'.ToString() },
                {".rar",'\uf1c6'.ToString() },
                {".7z",'\uf1c6'.ToString() },
                {".tar",'\uf1c6'.ToString() },
                {".gz",'\uf1c6'.ToString() },
                {".iso",'\uf1c6'.ToString() },
                {".dmg",'\uf1c6'.ToString() },
                {".img",'\uf1c6'.ToString() },
                {".mp3",'\uf1c7'.ToString() },
                {".wma",'\uf1c7'.ToString() },
                {".wav",'\uf1c7'.ToString() },
                {".ape",'\uf1c7'.ToString() },
                {".flac",'\uf1c7'.ToString() },
                {".ogg",'\uf1c7'.ToString() },
                {".aac",'\uf1c7'.ToString() },
                {".cs",'\uf1c9'.ToString() },
                {".css",'\uf1c9'.ToString() },
                {".html",'\uf1c9'.ToString() },
                {".js",'\uf1c9'.ToString() },
                {".ts",'\uf1c9'.ToString() },
                {".cc",'\uf1c9'.ToString() },
                {".h",'\uf1c9'.ToString() },
                {".c",'\uf1c9'.ToString() },
                {".hpp",'\uf1c9'.ToString() },
                {".hxx",'\uf1c9'.ToString() },
                {".cpp",'\uf1c9'.ToString() },
                {".cxx",'\uf1c9'.ToString() },
                {".xaml",'\uf1c9'.ToString() },
                {".php",'\uf1c9'.ToString() },
                {".jsp",'\uf1c9'.ToString() },
                {".jar",'\uf1c9'.ToString() },
                {".java",'\uf1c9'.ToString() },
                {".asp",'\uf1c9'.ToString() },
                {".aspx",'\uf1c9'.ToString() },
                {".class",'\uf1c9'.ToString() },
                {".go",'\uf1c9'.ToString() },
                {".xls",'\uf1c3'.ToString() },
                {".xlsx",'\uf1c3'.ToString() },
                {".xlsb",'\uf1c3'.ToString() },
                {".xlsm",'\uf1c3'.ToString() },
                {".csv",'\uf1c3'.ToString() },
                {".jpg",'\uf1c5'.ToString() },
                {".png",'\uf1c5'.ToString() },
                {".bmp",'\uf1c5'.ToString() },
                {".gif",'\uf1c5'.ToString() },
                {".tif",'\uf1c5'.ToString() },
                {".swf",'\uf1c5'.ToString() },
                {".ico",'\uf1c5'.ToString() },
                {".jpeg",'\uf1c5'.ToString() },
                {".pdf",'\uf1c1'.ToString() },
                {".ppt",'\uf1c4'.ToString() },
                {".pptx",'\uf1c4'.ToString() },
                {".pptm",'\uf1c4'.ToString() },
                {".ppsx",'\uf1c4'.ToString() },
                {".pps",'\uf1c4'.ToString() },
                {".avi",'\uf1c8'.ToString() },
                {".mp4",'\uf1c8'.ToString() },
                {".f4v",'\uf1c8'.ToString() },
                {".m4v",'\uf1c8'.ToString() },
                {".rmvb",'\uf1c8'.ToString() },
                {".mkv",'\uf1c8'.ToString() },
                {".mpg",'\uf1c8'.ToString() },
                {".mov",'\uf1c8'.ToString() },
                {".wmv",'\uf1c8'.ToString() },
                {".mpe",'\uf1c8'.ToString() },
                {".mpeg",'\uf1c8'.ToString() },
                {".doc",'\uf1c2'.ToString() },
                {".docx",'\uf1c2'.ToString() }
            };
        }

        public FileListItemViewModel(FileListViewModel parent, FileMetaData fileMetaData)
        {
            Parent = parent;
            Name = fileMetaData.Name;
            MTime = Calculators.UnixTimeStampConverter(fileMetaData.Mtime);
            ATime = Calculators.UnixTimeStampConverter(fileMetaData.Atime);
            CTime = Calculators.UnixTimeStampConverter(fileMetaData.Ctime);
            Locking = fileMetaData.Locking;
            Preview = fileMetaData.Preview;
            Directory = fileMetaData.Directory;
            Size = Calculators.SizeCalculator(fileMetaData.Size);
            UUID = fileMetaData.UUID;
            Mime = fileMetaData.Mime;
            Path = fileMetaData.Path;
            PreviewType = fileMetaData.PreviewType;

            CopyCommand = new DependencyCommand(Copy, AlwaysCan);
            CutCommand = new DependencyCommand(Cut, AlwaysCan);
            DeleteCommand = new AsyncCommand(Delete, AlwaysCan);
            RenameCommand = new DependencyCommand(Rename, AlwaysCan);
            ConfirmCommand = new AsyncCommand(Confirm, CanConfirm);
            DownloadCommand = new DependencyCommand(Download, CanDownload);
            MoreCommand = new DependencyCommand(More, AlwaysCan);

            if (Directory)
            {
                Icon = IconDictionary["folder"];
            }
            else
            {
                string eName = System.IO.Path.GetExtension(Name);
                Icon = IconDictionary.ContainsKey(eName) ? IconDictionary[eName] : IconDictionary["default"];
            }
        }
    }
}
