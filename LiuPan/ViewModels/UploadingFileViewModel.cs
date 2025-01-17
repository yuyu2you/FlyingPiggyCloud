﻿using EzWcs;
using EzWcs.Calculators;
using SixCloud.Controllers;
using System;
using System.IO;

namespace SixCloud.ViewModels
{
    internal class UploadingFileViewModel : UploadingTaskViewModel
    {
        public UploadingFileViewModel(string targetPath, string filePath) : base()
        {
            Name = Path.GetFileName(filePath);
            var hash = ETag.ComputeEtag(filePath);
            Models.GenericResult<Models.UploadToken> x = fileSystem.UploadFile(Name, parentPath: targetPath, Hash: hash, OriginalFilename: Name);
            if(x.Result.HashCached)
            {
                task = new HashCachedTask();
                return;
            }
            task = EzWcs.EzWcs.NewTask(filePath, x.Result.UploadInfo.Token, x.Result.UploadInfo.UploadUrl);
        }

        private readonly IUploadTask task;

        public override string Uploaded => Calculators.SizeCalculator(task.CompletedBytes);

        public override string Total => Calculators.SizeCalculator(task.TotalBytes);

        public override double Progress => task.CompletedBytes * 100 / task.TotalBytes;

        public override UploadStatus Status
        {
            get
            {
                if (task == null)
                {
                    return UploadStatus.Pause;
                }
                switch (task.UploadTaskStatus)
                {
                    case UploadTaskStatus.Active:
                        return UploadStatus.Running;
                    case UploadTaskStatus.Pause:
                    case UploadTaskStatus.Error:
                        return UploadStatus.Pause;
                    case UploadTaskStatus.Abort:
                        return UploadStatus.Stop;
                    case UploadTaskStatus.Completed:
                        return UploadStatus.Completed;
                }
                return UploadStatus.Running;
            }
        }

        public override void Stop(object parameter)
        {
            task.TaskOperate(UploadTaskStatus.Abort);
        }

        protected override void Pause()
        {
            task.TaskOperate(UploadTaskStatus.Pause);
        }

        protected override void Start()
        {
            task.TaskOperate(UploadTaskStatus.Active);
        }

        //public override event EventHandler UploadAborted;

        private class HashCachedTask : IUploadTask
        {
            public string FilePath { get; set; }

            public HashCachedTask()
            {
                CompletedBytes = 999;
                TotalBytes = 999;
            }

            public string Token { get; set; }

            public string Address { get; set; }

            public long CompletedBytes { get; set; }

            public string Hash { get; set; }

            public long TotalBytes { get; set; }

            public UploadTaskStatus UploadTaskStatus => UploadTaskStatus.Completed;

            public bool TaskOperate(UploadTaskStatus todo)
            {
                return false;
            }
        }

#if DEBUG
        ~UploadingFileViewModel()
        {
            Console.WriteLine("已回收");
        }
#endif

    }

}
