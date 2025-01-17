//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co. KG and its licensors">
// ?2016 Avira Operations GmbH & Co. KG and its licensors.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------

using FileDownloader.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace FileDownloader
{
    [DesignerCategory("Code")]
    internal class DownloadWebClient : WebClient
    {
        private readonly ILogger logger = LoggerFacade.GetCurrentClassLogger();
        private readonly CookieContainer cookieContainer = new CookieContainer();
        private WebResponse webResponse;
        private long position;

        private TimeSpan timeout = TimeSpan.FromMinutes(2);

        public bool HasResponse => webResponse != null;

        public bool IsPartialResponse
        {
            get
            {
                HttpWebResponse response = webResponse as HttpWebResponse;
                return response != null && response.StatusCode == HttpStatusCode.PartialContent;
            }
        }

        public void OpenReadAsync(Uri address, long newPosition)
        {
            position = newPosition;
            OpenReadAsync(address);
        }

        public string GetOriginalFileNameFromDownload()
        {
            if (webResponse == null)
            {
                return null;
            }

            try
            {
                System.Net.Mime.ContentDisposition contentDisposition = webResponse.Headers.GetContentDisposition();
                if (contentDisposition != null)
                {
                    string filename = contentDisposition.FileName;
                    if (!string.IsNullOrEmpty(filename))
                    {
                        return Path.GetFileName(filename);
                    }
                }
                return Path.GetFileName(webResponse.ResponseUri.LocalPath);
            }
            catch (Exception ex)
            {
                logger.Warn("Can't get the name of the downloading file. Exception: {0}", ex.Message);
                return null;
            }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            webResponse = response;
            logger.Debug("Recieved web response for sync call, IsFromCache: {0}, url {1}", response != null && response.IsFromCache, request.RequestUri);
            return response;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse response = base.GetWebResponse(request, result);
            webResponse = response;

            logger.Debug("Recieved web response for async call, IsFromCache: {0}, url {1}", response != null && response.IsFromCache, request.RequestUri);
            return response;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request != null)
            {
                request.Timeout = (int)timeout.TotalMilliseconds;
            }

            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest == null)
            {
                return request;
            }

            webRequest.ReadWriteTimeout = (int)timeout.TotalMilliseconds;
            webRequest.Timeout = (int)timeout.TotalMilliseconds;
            if (position != 0)
            {
                webRequest.AddRange((int)position);
                webRequest.Accept = "*/*";
            }
            webRequest.CookieContainer = cookieContainer;
            return request;
        }
    }
}