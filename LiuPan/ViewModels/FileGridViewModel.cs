﻿using SixCloud.Models;
using SixCloud.Views.UserControls;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SixCloud.ViewModels
{
    internal class FileGridViewModel : FileListViewModel
    {
        public Mode Mode { get; set; } = Mode.FileListContainer;

        public object SelectObject { get; set; }

        protected override async Task GetFileListByPath(string path)
        {
            IEnumerable<FileMetaData[]> GetFileList()
            {
                int currentPage = 0;
                int totalPage;
                do
                {
                    GenericResult<FileListPage> x = fileSystem.GetDirectory(path: path, page: ++currentPage);
                    if (x.Success && x.Result.DictionaryInformation != null)
                    {
                        totalPage = x.Result.TotalPage;
                        CurrentPath = x.Result.DictionaryInformation.Path;
                        CurrentUUID = x.Result.DictionaryInformation.UUID;
                        CreatePathArray(CurrentPath);
                        yield return x.Result.List;
                    }
                    else
                    {
                        throw new DirectoryNotFoundException(x.Message);
                    }
                } while (currentPage < totalPage);
                yield break;
            }

            App.Current.Dispatcher.Invoke(() => FileList.Clear());
            await Task.Run(() =>
            {
                fileMetaDataEnumerator = GetFileList().GetEnumerator();
                fileMetaDataEnumerator.MoveNext();
            });

            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (FileMetaData a in fileMetaDataEnumerator.Current)
                {
                    if (Mode == Mode.PathSelector && !a.Directory)
                    {
                        continue;
                    }
                    FileList.Add(new FileListItemViewModel(this, a));
                }
            });
        }

        protected override async Task GetFileListByUUID(string uuid)
        {
            {
                IEnumerable<FileMetaData[]> GetFileList()
                {
                    int currentPage = 0;
                    int totalPage;
                    do
                    {
                        GenericResult<FileListPage> x = fileSystem.GetDirectory(uuid, page: ++currentPage);
                        if (x.Success)
                        {
                            totalPage = x.Result.TotalPage;
                            CurrentPath = x.Result.DictionaryInformation.Path;
                            CurrentUUID = x.Result.DictionaryInformation.UUID;
                     
       CreatePathArray(CurrentPath);
                            yield return x.Result.List;
                        }
                        else
                        {
                            throw new DirectoryNotFoundException(x.Message);
                        }
                    } while (currentPage < totalPage);
                    yield break;
                }


                Application.Current.Dispatcher.Invoke(() => FileList.Clear());
                await Task.Run(() =>
                {
                    fileMetaDataEnumerator = GetFileList().GetEnumerator();
                    fileMetaDataEnumerator.MoveNext();
                });
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (FileMetaData a in fileMetaDataEnumerator.Current)
                    {
                        if (Mode == Mode.PathSelector && !a.Directory)
                        {
                            continue;
                        }
                        FileList.Add(new FileListItemViewModel(this, a));
                    }
                });
            }
        }

        public DependencyCommand NavigateCommand { get; set; }
        private async void Navigate(object parameter)
        {
            var selectObject = (FileListItemViewModel)parameter;
            await GetFileListByUUID(selectObject.UUID);
            LazyLoad();
        }

        public FileGridViewModel()
        {
            NavigateCommand = new DependencyCommand(Navigate, DependencyCommand.AlwaysCan);
            Task.Run(async () =>
            {
                await GetFileListByPath("/");
            });
        }
    }
}