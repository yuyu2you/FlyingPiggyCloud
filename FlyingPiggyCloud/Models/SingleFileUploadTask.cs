﻿using FlyingPiggyCloud.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wangsu.WcsLib.Core;
using WcsLib.Core;

namespace FlyingPiggyCloud.Models
{
    public class SingleFileUploadTask:INotifyPropertyChanged,IUploadTask
    {
        private static readonly FileSystemMethods fileSystemMethods = new FileSystemMethods(Properties.Settings.Default.BaseUri);

        /// <summary>
        /// 待上传文件的绝对路径
        /// </summary>
        private readonly string fullPath;

        /// <summary>
        /// 待上传文件的文件名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 已上传的字节数
        /// </summary>
        public long UploadedBytes { get; private set; }

        public string Uploaded => Calculators.SizeCalculator(UploadedBytes);

        /// <summary>
        /// 文件的总字节数
        /// </summary>
        public long TotalBytes { get; private set; }

        public string Total => Calculators.SizeCalculator(TotalBytes);

        /// <summary>
        /// 上传进度
        /// </summary>
        public double Progress
        {
            get
            {
                if(TotalBytes!=0)
                {
                    return UploadedBytes * 100 / TotalBytes;
                }
                else
                {
                    return 0D;
                }
            }
        }

        /// <summary>
        /// 开始上传任务
        /// </summary>
        /// <param name="parentUUID">目标目录的UUID</param>
        public async Task StartTask(string parentUUID=null,string parentPath=null)
        {
            Controllers.Results.ResponesResult<Controllers.Results.FileSystem.UploadResponseResult> x = await fileSystemMethods.UploadFile(FileName, parentUUID,parentPath, OriginalFilename: FileName);
            UploadProgressHandler uploadProgressHandler = new UploadProgressHandler((a, b) =>
              {
                  App.Current.Dispatcher.Invoke(() =>
                  {
                      UploadedBytes = a;
                      TotalBytes = b;
                      OnPropertyChanged("Uploaded");
                      OnPropertyChanged("Total");
                      OnPropertyChanged("Progress");
                  });
              });
            await Task.Run(() =>
            {
                try
                {
#if DEBUG
                    EzWcs.EzWcs.NewTask(fullPath, x.Result.Token, x.Result.UploadUrl);
#else
                    Upload.Start(x.Result.Token, fullPath, x.Result.UploadUrl, FileName, uploadProgressHandler: uploadProgressHandler, userCommand: uploadTaskOperator);
#endif
                    Status = "上传成功";
                    OnPropertyChanged("Status");
                    OnTaskCompleted?.Invoke(this, new EventArgs());
                    
                }
                catch (Exception ex)
                {
                    Status = ex.Message;
                    //这里应该修改其他属性，暂时先这么解决
                    UploadedBytes = TotalBytes;
                    OnPropertyChanged("Uploaded");
                    OnPropertyChanged("Total");
                    OnPropertyChanged("Progress");
                    OnPropertyChanged("Status");
                }
            });
        }

        private readonly UploadTaskOperator uploadTaskOperator;

        /// <summary>
        /// 取消上传任务，这个指令会在当前块上传结束后执行，如果待上传文件只有一个块则该指令可能无法生效
        /// </summary>
        public void Cancel()
        {
            //Status = "上传任务被用户取消";
            //OnPropertyChanged("Status");
            uploadTaskOperator.Cancle();
        }

        public string Status { get; set; }

        /// <summary>
        /// 实例化一个上传任务
        /// </summary>
        /// <param name="fullPath">文件的路径</param>
        /// <param name="FileName">文件名</param>
        public SingleFileUploadTask(string fullPath, string FileName)
        {
            this.fullPath = fullPath;
            this.FileName = FileName;
            UploadedBytes = 0;
            TotalBytes = 0;
            uploadTaskOperator = new UploadTaskOperator();
#if DEBUG
            Status = "新鲜热乎的上传任务";
#endif
        }
        
        public event TaskStatusChangedEventHandler OnTaskCompleted;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }

}
