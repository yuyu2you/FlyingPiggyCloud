﻿using SixCloud.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace SixCloud.ViewModels
{
    internal class FileListViewModel : AFileSystemVIewModel
    {
        public string[] CopyList { get; set; }

        public string[] CutList { get; set; }

        public List<string> PathArray { get; set; } = new List<string>();

        public ObservableCollection<FileListItemViewModel> FileList { get; set; } = new ObservableCollection<FileListItemViewModel>();

        public string CurrentPath { get; set; }

        public string CurrentUUID { get; set; }

        #region FileListViewModelFunctions
        /// <summary>
        /// 保存LazyLoad状态的枚举器
        /// </summary>
        private IEnumerator<FileMetaData[]> fileMetaDataEnumerator;

        public void GetFileListByPath(string path)
        {
            IEnumerable<FileMetaData[]> GetFileList()
            {
                int currentPage;
                int totalPage;
                do
                {
                    GenericResult<FileListPage> x = fileSystem.GetDirectory("", path);
                    if (x.Success)
                    {
                        currentPage = x.Result.Page;
                        totalPage = x.Result.TotalPage;
                        yield return x.Result.List;
                    }
                    else
                    {
                        throw new DirectoryNotFoundException(x.Message);
                    }
                } while (currentPage < totalPage);
                yield break;
            }
            void callback()
            {
                fileMetaDataEnumerator.MoveNext();
                foreach (FileMetaData a in fileMetaDataEnumerator.Current)
                {
                    FileList.Add(new FileListItemViewModel(a));
                }
            }

            FileList.Clear();
            fileMetaDataEnumerator = GetFileList().GetEnumerator();
            App.Current.Dispatcher.Invoke(callback);
        }

        public void GetFileListByUUID(string uuid)
        {
            IEnumerable<FileMetaData[]> GetFileList()
            {
                int currentPage;
                int totalPage;
                do
                {
                    GenericResult<FileListPage> x = fileSystem.GetDirectory(uuid);
                    if (x.Success)
                    {
                        currentPage = x.Result.Page;
                        totalPage = x.Result.TotalPage;
                        yield return x.Result.List;
                    }
                    else
                    {
                        throw new DirectoryNotFoundException(x.Message);
                    }
                } while (currentPage < totalPage);
                yield break;
            }
            void callback()
            {
                fileMetaDataEnumerator.MoveNext();
                foreach (FileMetaData a in fileMetaDataEnumerator.Current)
                {
                    FileList.Add(new FileListItemViewModel(a));
                }
            }
            FileList.Clear();
            fileMetaDataEnumerator = GetFileList().GetEnumerator();
            App.Current.Dispatcher.Invoke(callback);
        }

        public void LazyLoad()
        {
            if (fileMetaDataEnumerator != null)
            {
                try
                {
                    if (fileMetaDataEnumerator.MoveNext())
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (FileMetaData a in fileMetaDataEnumerator.Current)
                            {
                                FileList.Add(new FileListItemViewModel(a));
                            }
                        });
                    }
                    else
                    {
                        fileMetaDataEnumerator = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "加载文件列表失败");
                }
            }

        }

        private void CreatePathArray(string path)
        {
            if (path == "/")
            {
                PathArray = new List<string>(new string[]
                {
                    "root"
                });
            }
            else
            {
                PathArray = new List<string>(System.Text.RegularExpressions.Regex.Split(path, "/", System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace));
            }
            OnPropertyChanged("PathArray");
        }
        #endregion

        #region NavigationCommand

        #region PreviousNavigate
        public DependencyCommand PreviousNavigateCommand { get; private set; }

        private async void PreviousNavigate(object parameter)
        {
            nextPath.Push(CurrentPath);
            if (previousPath.Count > 0)
            {
                bool success;
                do
                {
                    try
                    {
                        string path = previousPath.Pop();
                        await Task.Run(() =>
                        {
                            GetFileListByPath(path);
                        });
                        success = true;
                        CreatePathArray(path);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        success = false;
                    }
                } while (!success && previousPath.Count != 0);
            }
            PreviousNavigateCommand.OnCanExecutedChanged(this, new EventArgs());
            NextNavigateCommand.OnCanExecutedChanged(this, new EventArgs());
        }

        private bool CanPreviousNavigate(object parameter)
        {
            return previousPath.Count != 0 ? true : false;
        }

        /// <summary>
        /// 为后退按钮保存历史路径
        /// </summary>
        private Stack<string> previousPath = new Stack<string>();
        #endregion

        #region NextNavigate
        public DependencyCommand NextNavigateCommand { get; private set; }

        private async void NextNavigate(object parameter)
        {
            previousPath.Push(CurrentPath);
            if (nextPath.Count > 0)
            {
                bool success;
                do
                {
                    try
                    {
                        string path = nextPath.Pop();
                        await Task.Run(() =>
                        {
                            GetFileListByPath(path);
                        });
                        success = true;
                        CreatePathArray(path);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        success = false;
                    }
                } while (!success && nextPath.Count != 0);
            }
            PreviousNavigateCommand.OnCanExecutedChanged(this, new EventArgs());
            NextNavigateCommand.OnCanExecutedChanged(this, new EventArgs());
        }

        private bool CanNextNavigate(object parameter)
        {
            return nextPath.Count != 0 ? true : false;
        }

        /// <summary>
        /// 为前进按钮保存历史路径
        /// </summary>
        private Stack<string> nextPath = new Stack<string>();
        #endregion
        #endregion

        #region FileOperationCommand
        #region Stick
        public DependencyCommand StickCommand { get; private set; }

        private async void Stick(object parameter)
        {
            await Task.Run(new Action(callback));

            void callback()
            {
                if (CopyList != null && CopyList.Length > 0)
                {
                    string[] copyList = CopyList;
                    CopyList = null;
                    StickCommand.OnCanExecutedChanged(this, new EventArgs());
                    foreach (string a in copyList)
                    {
                        fileSystem.Copy(a, CurrentUUID);
                    }
                }
                else if (CutList != null && CutList.Length > 0)
                {
                    string[] cutList = CutList;
                    CutList = null;
                    StickCommand.OnCanExecutedChanged(this, new EventArgs());
                    foreach (string a in cutList)
                    {
                        fileSystem.Move(a, CurrentUUID);
                    }
                }
                else
                {
                    CutList = null;
                    CopyList = null;
                    StickCommand.OnCanExecutedChanged(this, new EventArgs());
                }
            }

        }

        private bool CanStick(object parameter)
        {
            if ((CopyList != null && CopyList.Length > 0) || (CutList != null && CutList.Length > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Cut
        public DependencyCommand CutCommand { get; private set; }

        private void Cut(object parameter)
        {
            if (parameter is IList selectedItems)
            {
                List<string> list = new List<string>(selectedItems.Count);
                foreach (FileListItemViewModel a in selectedItems)
                {
                    list.Add(a.UUID);
                }
                CutList = list.ToArray();
                CopyList = null;
                StickCommand.OnCanExecutedChanged(this, new EventArgs());
            }
        }

        private bool CanCut(object parameter)
        {
            return true;
        }
        #endregion

        #region Copy
        public DependencyCommand CopyCommand { get; private set; }

        private void Copy(object parameter)
        {
            if (parameter is IList selectedItems)
            {
                List<string> list = new List<string>(selectedItems.Count);
                foreach (FileListItemViewModel a in selectedItems)
                {
                    list.Add(a.UUID);
                }
                CopyList = list.ToArray();
                CutList = null;
                StickCommand.OnCanExecutedChanged(this, new EventArgs());
            }
        }

        private bool CanCopy(object parameter)
        {
            return true;
        }
        #endregion


        #endregion

        public FileListViewModel()
        {
            NextNavigateCommand = new DependencyCommand(NextNavigate, CanNextNavigate);
            PreviousNavigateCommand = new DependencyCommand(PreviousNavigate, CanPreviousNavigate);
            StickCommand = new DependencyCommand(Stick, CanStick);
            CutCommand = new DependencyCommand(Cut, CanCut);
            CopyCommand = new DependencyCommand(Copy, CanCopy);
        }
    }
}