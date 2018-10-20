﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace FlyingAria2c
{
    internal static class Aria2cMethords
    {
        /// <summary>
        /// Aria2 Rpc接口的方法库
        /// </summary>
        public static class Aria2Methords
        {
            private static string Base64Encode(string Path)
            {
                try
                {
                    return Convert.ToBase64String(File.ReadAllBytes(Path));
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }

            /// <summary>
            /// 新建一个任务
            /// </summary>
            /// <param name="Uri">下载链接的地址</param>
            /// <returns></returns>
            public static async Task<string> AddUri(JsonRpcConnection Connection, string Uri)
            {
                string[] Uris = { Uri };
                object[] Params = { "token:" + Connection.Token, Uris };
                string Gid = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.addUri", Params)).Result;
                return Gid;
            }

            /// <summary>
            /// 新建一个任务，包含多个源
            /// </summary>
            /// <param name="Uris">这个包含多个下载链接的数组指向同一个文件</param>
            /// <returns></returns>
            public static async Task<string> AddUri(JsonRpcConnection Connection, string[] Uris)
            {
                object[] Params = { "token:" + Connection.Token, Uris };
                string Gid = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.addUri", Params)).Result;
                return Gid;
            }

            /// <summary>
            /// 添加种子
            /// </summary>
            /// <param name="Path">种子文件在计算机上的位置</param>
            /// <returns></returns>
            public static async Task<string> AddTorrent(JsonRpcConnection Connection, string Path)
            {
                string TorrentBase64 = Base64Encode(Path);
                object[] Params = { "token:" + Connection.Token, TorrentBase64 };
                string Gid = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.addTorrent", Params)).Result;
                return Gid;
            }

            /// <summary>
            /// 添加MetaLink
            /// </summary>
            /// <param name="Path">MetaLink文件在计算机上的位置</param>
            /// <returns></returns>
            public static async Task<string> AddMetalink(JsonRpcConnection Connection, string Path)
            {
                string MetalinkBase64 = Base64Encode(Path);
                object[] Params = { "token:" + Connection.Token, MetalinkBase64 };
                string Gid = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.addMetalink", Params)).Result;
                return Gid;
            }

            public static async Task<string> Remove(JsonRpcConnection Connection, string Gid)
            {
                //string[] Gids = new string[] { Gid };
                object[] Params = { "token:" + Connection.Token, Gid };
                string Result = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.remove", Params)).Result;
                return Result;
            }

            public static async Task<string> Pause(JsonRpcConnection Connection, string Gid)
            {
                object[] Params = { "token:" + Connection.Token, Gid };
                string Result = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.pause", Params)).Result;
                return Result;
            }

            public static async Task PauseAll(JsonRpcConnection Connection)
            {
                object[] Params = { "token:" + Connection.Token };
                await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.pauseAll", Params);
            }

            public static async Task<string> UpPause(JsonRpcConnection Connection, string Gid)
            {
                object[] Params = { "token:" + Connection.Token, Gid };
                string Result = (await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.unpause", Params)).Result;
                return Result;
            }

            public static async Task UpPauseAll(JsonRpcConnection Connection)
            {
                object[] Params = { "token:" + Connection.Token };
                await Connection.JsonRpcAsync<JsonRpcConnection.Response<string>>("aria2.unpauseAll", Params);
            }

            public static async Task<JRCtler.JsonRpcRes> TellStatus(JsonRpcConnection Connection, string Gid)
            {
                string[] Keys = new string[] { "status", "totalLength", "completedLength", "downloadSpeed", "gid" };
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
                Gid,
                Keys
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.tellStatus", Params);
                return Result;
            }

            public static async Task<JRCtler.JsonRpcRes> TellStatus(JsonRpcConnection Connection, string Gid, string[] Keys)
            {
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
                Gid,
                Keys
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.tellStatus", Params);
                return Result;
            }

            public static async Task<JRCtler.JsonRpcRes> TellActive(JsonRpcConnection Connection)
            {
                string[] Keys = new string[] { "status", "totalLength", "completedLength", "downloadSpeed", "gid" };
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
                Keys
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.tellActive", Params);
                return Result;
            }

            public static async Task<JRCtler.JsonRpcRes> TellWaiting(JsonRpcConnection Connection)
            {
                string[] Keys = new string[] { "status", "totalLength", "completedLength", "downloadSpeed", "gid" };
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
                0,
                50,
                Keys
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.tellWaiting", Params);
                return Result;
            }

            /// <summary>
            /// 查询已停止的任务
            /// </summary>
            /// <returns>返回最近50个结果</returns>
            public static async Task<JRCtler.JsonRpcRes> TellStopped(JsonRpcConnection Connection)
            {
                string[] Keys = new string[] { "status", "totalLength", "completedLength", "downloadSpeed", "gid" };
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
                0,
                50,
                Keys
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.tellStopped", Params);
                return Result;
            }

            public static async Task<JRCtler.JsonRpcRes> GetFiles(JsonRpcConnection Connection, string Gid)
            {
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
                Gid
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.getFiles", Params);
                return Result;
            }

            public static async Task<JRCtler.JsonRpcRes> GetGlobalStat(JsonRpcConnection Connection)
            {
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
            };
                JRCtler.JsonRpcRes Result = await Stc.Line.JsonRpcAsync("aria2.getGlobalStat", Params);
                return Result;

            }

            /// <summary>
            /// 关闭Aria2C，调用Aria2自己的方法
            /// </summary>
            public static void ShutDown(JsonRpcConnection Connection)
            {
                object[] Params = new object[]
            {
                "token:" + Stc.GloConf.Rpc_secret,
            };
                Connection.JsonRpcWithoutRes("aria2.shutdown", Params);
                Stc.Line.Quit();
            }

        }
    }
}
