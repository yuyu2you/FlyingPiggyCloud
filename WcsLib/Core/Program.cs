﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Wangsu.WcsLib.HTTP;
using Wangsu.WcsLib.Utility;

namespace Wangsu.WcsLib.Core
{
    public class Program
    {
        private const int DATASIZE = 16 * 1024 * 1024;
        private const int BLOCKSIZE = 4 * 1024 * 1024;
        private const int FIRSTCHUNKSIZE = 1024;

        static void Main(string uploadToken, string FilePath, string UploadUrl)
        {

            List<string> TotelContexts = new List<string>();

            Config config = new Config(UploadUrl);

            FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            
            byte[] data = binaryReader.ReadBytes(BLOCKSIZE);

            // 最后合成文件时的 hash
            //Console.WriteLine("ETag of uploading data: {0}", ETag.ComputeEtag(data));

            // 第一个分片不宜太大，因为可能遇到错误，上传太大是白费流量和时间！

            SliceUpload su = new SliceUpload(config);
            HttpResult result = su.MakeBlock(BLOCKSIZE, 0, data, 0, FIRSTCHUNKSIZE, uploadToken);
            Console.WriteLine("---MakeBlock---\n{0}", result.ToString());

            if ((int)HttpStatusCode.OK == result.Code)
            {
                long blockCount = (DATASIZE + BLOCKSIZE - 1) / BLOCKSIZE;
                List<string> TotalContexts = new List<string>();
                //string[] contexts = new string[blockCount];

                JObject jo = JObject.Parse(result.Text);

                contexts[0] = jo["ctx"].ToString();

                // 上传第 1 个 block 剩下的数据
                result = su.Bput(contexts[0], FIRSTCHUNKSIZE, data, FIRSTCHUNKSIZE, (int)(BLOCKSIZE - FIRSTCHUNKSIZE), uploadToken);
                Console.WriteLine("---Bput---\n{0}", result.ToString());
                if ((int)HttpStatusCode.OK == result.Code)
                {
                    jo = JObject.Parse(result.Text);
                    contexts[0] = jo["ctx"].ToString();

                    //// 上传后续 block，每次都是一整块上传
                    //for (int blockIndex = 1; blockIndex < blockCount; ++blockIndex)
                    //{
                    //    long leftSize = DATASIZE - BLOCKSIZE * blockIndex;
                    //    int chunkSize = (int)(leftSize > BLOCKSIZE ? BLOCKSIZE : leftSize);
                    //    result = su.MakeBlock(chunkSize, blockIndex, data, (int)(BLOCKSIZE * blockIndex), chunkSize, uploadToken);
                    //    //Console.WriteLine("---MakeBlock---\n{0}", result.ToString());
                    //    if ((int)HttpStatusCode.OK == result.Code)
                    //    {
                    //        jo = JObject.Parse(result.Text);
                    //        contexts[blockIndex] = jo["ctx"].ToString();
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("----Exit with error----");
                    //        return;
                    //    }
                    //}

                    // 合成文件，注意与前面打印的 ETag 对比
                    result = su.MakeFile(DATASIZE, null, contexts, uploadToken);
                    Console.WriteLine("---MakeFile---\n{0}", result.ToString());
                }
            }
        }

        private static string[] UploadBlocks(byte[] data, SliceUpload su,string uploadToken)
        {
            long blockCount = (data.LongLength + BLOCKSIZE - 1) / BLOCKSIZE;
            List<string> TotalContexts = new List<string>();
            string[] contexts = new string[blockCount];

            // 上传后续 block，每次都是一整块上传
            for (int blockIndex = 0; blockIndex < blockCount; ++blockIndex)
            {
                

                long leftSize = DATASIZE - BLOCKSIZE * blockIndex;
                int chunkSize = (int)(leftSize > BLOCKSIZE ? BLOCKSIZE : leftSize);
                var result = su.MakeBlock(chunkSize, blockIndex, data, (int)(BLOCKSIZE * blockIndex), chunkSize, uploadToken);
                //Console.WriteLine("---MakeBlock---\n{0}", result.ToString());
                if ((int)HttpStatusCode.OK == result.Code)
                {
                    JObject jo = JObject.Parse(result.Text);
                    contexts[blockIndex] = jo["ctx"].ToString();
                }
                else
                {
                    throw new Exception("Exit with error");
                }
            }
            return contexts;
        }
    }
}
