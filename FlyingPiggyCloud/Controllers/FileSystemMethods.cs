﻿using FlyingPiggyCloud.Controllers.Results;
using FlyingPiggyCloud.Controllers.Results.FileSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlyingPiggyCloud.Controllers
{
    public class FileSystemMethods : QingzhenyunRequestBase
    {
        public FileSystemMethods(string BaseUri) : base(BaseUri)
        {

        }

        /// <summary>
        /// 请求的时候，如果 name 和 path均为空，则会返回根目录
        /// </summary>
        /// <param name="Parent">该文件夹的ID</param>
        /// <param name="Path">路径</param>
        /// <param name="Page">第几页</param>
        /// <param name="PageSize">列表大小，最大值999</param>
        /// <param name="OrderBy">排序：0按文件名，1按时间</param>
        /// <param name="Type">文件类型：0显示文件，1显示文件夹，-1显示文件和文件夹(默认)</param>
        /// <returns></returns>
        public async Task<PageResponseResult> GetDirectory(string Parent="", string Path = "", int? Page = null, int? PageSize = null , int? OrderBy = null, int? Type = null)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if(Parent!="")
            {
                data.Add("parent", Parent);
            }
            if (Path != "")
            {
                data.Add("path", Path);
            }
            if (Page != null)
            {
                data.Add("page", Page);
            }
            if (PageSize != null)
            {
                data.Add("pageSize", PageSize);
            }
            if (OrderBy != null)
            {
                data.Add("orderBy", OrderBy);
            }
            if (Type != null)
            {
                data.Add("type", Type);
            }
            if(Token==null)
            {
                var GetToken = new Views.LoginWindow();
                GetToken.ShowDialog();
            }
            data.Add("token", Token);
            var x = await PostAsync<PageResponseResult>(JsonConvert.SerializeObject(data), "v1/files/page");
            Token = x.Token;
            return x;
        }

        /// <summary>
        /// 新建文件夹，Parent和Path参数不可共存
        /// </summary>
        /// <param name="Name">新文件夹的名字，如果其他参数为空则在根目录创建此文件夹</param>
        /// <param name="Parent">在UUID为此的文件夹内创建新文件夹</param>
        /// <param name="Path">在此路径下创建新文件夹，如果Name参数为空则创建此路径</param>
        /// <returns></returns>
        public async Task <GetMetaDataResponseResult> CreatDirectory(string Name="", string Parent="", string Path="")
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if(Name!="")
            {
                data.Add("name", Name);
            }
            if(Parent!="")
            {
                data.Add("parent", Parent);
            }
            else if(Path!="")
            {
                data.Add("path", Path);
            }
            if (Token == null)
            {
                var GetToken = new Views.LoginWindow();
                GetToken.ShowDialog();
            }
            data.Add("token", Token);
            var x = await PostAsync<GetMetaDataResponseResult>(JsonConvert.SerializeObject(data), "v1/files/createDirectory");
            Token = x.Token;
            return x;
        }

        /// <summary>
        /// 移动文件夹或文件
        /// </summary>
        /// <param name="SourceMeta">被移动的项目</param>
        /// <param name="TargetDirectory">目标位置，必须是一个文件夹的Meta信息</param>
        /// <returns></returns>
        public async Task<ResponesResult<bool>> Move(FileMetaData SourceMeta, FileMetaData TargetDirectory)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "uuid", SourceMeta.UUID },
                { "parent", TargetDirectory.UUID }
            };
            if (Token == null)
            {
                var GetToken = new Views.LoginWindow();
                GetToken.ShowDialog();
            }
            data.Add("token", Token);
            var x = await PostAsync<ResponesResult<bool>>(JsonConvert.SerializeObject(data), "v1/files/move");
            Token = x.Token;
            return x;
        }

        /// <summary>
        /// 复制文件夹或文件
        /// </summary>
        /// <param name="SourceMeta">被复制的项目</param>
        /// <param name="TargetDirectory">目标位置，必须是一个文件夹的Meta信息</param>
        /// <returns></returns>
        public async Task<ResponesResult<bool>> Copy(FileMetaData SourceMeta, FileMetaData TargetDirectory)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "uuid", SourceMeta.UUID },
                { "parent", TargetDirectory.UUID }
            };
            if (Token == null)
            {
                var GetToken = new Views.LoginWindow();
                GetToken.ShowDialog();
            }
            data.Add("token", Token);
            var x = await PostAsync<ResponesResult<bool>>(JsonConvert.SerializeObject(data), "v1/files/copy");
            Token = x.Token;
            return x;
        }

        /// <summary>
        /// 删除文件夹或文件
        /// </summary>
        /// <param name="SourceMeta">被删除的项目</param>
        /// <returns></returns>
        public async Task<ResponesResult<bool>> Remove(FileMetaData SourceMeta)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "uuid", SourceMeta.UUID }
            };
            if (Token == null)
            {
                var GetToken = new Views.LoginWindow();
                GetToken.ShowDialog();
            }
            data.Add("token", Token);
            var x = await PostAsync<ResponesResult<bool>>(JsonConvert.SerializeObject(data), "v1/files/remove");
            Token = x.Token;
            return x;
        }

        /// <summary>
        /// 重命名文件夹或文件
        /// </summary>
        /// <param name="SourceMeta">被重命名的项目</param>
        /// <param name="NewName">新名称</param>
        /// <returns></returns>
        public async Task<GetMetaDataResponseResult> Rename(FileMetaData SourceMeta, string NewName)
        {
            if (Token == null)
            {
                var GetToken = new Views.LoginWindow();
                GetToken.ShowDialog();
            }
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "uuid", SourceMeta.UUID },
                { "name", NewName },
                { "token", Token }
            };
            var x = await PostAsync<GetMetaDataResponseResult>(JsonConvert.SerializeObject(data), "v1/files/rename");
            Token = x.Token;
            return x;
        }
    }
}