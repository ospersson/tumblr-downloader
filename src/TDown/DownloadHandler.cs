using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace TDown
{
    public class DownloadHandler
    {
        /// <summary>
        /// Download avatar from Tumblr blog. Using open method from Tumblr API V2.
        /// </summary>
        /// <param name="siteName">Name of Tumblr blog</param>
        /// <param name="folderPath">Save avatar to this path</param>
        public void DownloadAvatar(string siteName, string folderPath)
        {
            string avatarUrl = @"https://api.tumblr.com/v2/blog/" + siteName + "/avatar/512";

            var avatarDiskPath = folderPath + "\\Avatar.jpg";

            if (File.Exists(avatarDiskPath)) return;

            DownloadRemoteImageFile(avatarUrl, avatarDiskPath);
        }

        public void DownloadAllImages(IList<Post> posts, string folderPath, int maxWait = 5000)
        {
            var imageNbr = 1;
            var rnd = new Random();

            foreach (var post in posts)
            {
                string imagePath = folderPath + "\\" + post.ImageName();

                if (File.Exists(imagePath))
                {
                    var consoleInfoExist = "Image " + imageNbr + " of 50: " + post.ImageName() + " exists on disk   ";
                    //Random sleep, possible to read the text!
                    Thread.Sleep(rnd.Next(maxWait / 5));
                    StatusMsg(string.Format("{0}", consoleInfoExist));
                    imageNbr++;
                    continue;
                }

                //Save each image to disk.
                DownloadRemoteImageFile(post.PhotoUrl1280, imagePath);

                var consoleInfo = "Dowloading image " + imageNbr + " of 50: " + post.ImageName();
                //Random sleep, be nice to the host!
                Thread.Sleep(rnd.Next(maxWait));
                StatusMsg(string.Format("{0}", consoleInfo));

                imageNbr++;
            }
        }

        /// <summary>
        /// Download a image from url. 
        /// 
        /// Modified code from Stackoverflow. 
        /// http://stackoverflow.com/questions/3615800/download-image-from-the-site-in-net-c/12631127#12631127
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response;

            request.UserAgent = "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10.8.2; en-US) tdown/1.0.0 Sunflower/1.61803398";

            WebHeaderCollection whcollection = new WebHeaderCollection();
            whcollection.Set("Love-You-Guys", "Downloading some nice pictures! Thanks!!");
            request.Headers = whcollection;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                throw;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {

                // if the remote file was found, download it
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                return true;
            }
            else
                return false;
        }

        public void StatusMsg(string msg)
        {
            StatusMessageEventArgs args = new StatusMessageEventArgs();
            args.Message = msg;
            OnStatusMessage(args);
        }

        protected virtual void OnStatusMessage(StatusMessageEventArgs e)
        {
            StatusMessageEventHandler handler = DownloadStatusMessage;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event StatusMessageEventHandler DownloadStatusMessage;

    }

    public class StatusMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public delegate void StatusMessageEventHandler(object sender, StatusMessageEventArgs e);
}
