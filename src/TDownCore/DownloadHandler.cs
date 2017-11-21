using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace TDownCore
{
    public interface IDownloadHandler
    {
        void DownloadAvatar(string siteName, string folderPath);
        void DownloadAllImages(IList<Post> posts, string folderPath, int maxWait = 5000);
        event StatusMessageEventHandler DownloadStatusMessage;
    }

    public class DownloadHandler : IDownloadHandler
    {
        /// <summary>
        /// Download avatar from Tumblr blog. Using open method from Tumblr API V2.
        /// </summary>
        /// <param name="siteName">Name of Tumblr blog</param>
        /// <param name="folderPath">Save avatar to this path</param>
        public async void DownloadAvatar(string siteName, string folderPath)
        {
            string avatarUrl = @"https://api.tumblr.com/v2/blog/" + siteName + "/avatar/512";

            var avatarDiskPath = folderPath + "\\Avatar_" + siteName + ".jpg";

            if (File.Exists(avatarDiskPath)) return;

            await DownloadRemoteImageFile(avatarUrl, avatarDiskPath);
        }

        public async void DownloadAllImages(IList<Post> posts, string folderPath, int maxWait = 10000)
        {
            var imageNbr = 1;
            var rnd = new Random();

            foreach (var post in posts)
            {
                string consoleInfo = string.Empty;
                string imagePath = string.Empty;

                var nbrPhotosInPost = 0;

                //Check if it is a multiple image post.
                foreach (var photo in post.Photos)
                {
                    imagePath = folderPath + "\\" + post.ImageName(photo.PhotoUrl1280);

                    if (File.Exists(photo.PhotoUrl1280))
                    {
                        var consoleInfoExist = "Image " + imageNbr + " of 50: " + post.ImageName(photo.PhotoUrl1280) + " exists on disk   ";
                        //Random sleep, possible to read the text!
                        Thread.Sleep(rnd.Next(maxWait / 5));
                        StatusMsg(string.Format("{0}", consoleInfoExist));
                        imageNbr++;
                        continue;
                    }

                    //Random sleep, be nice to the host!
                    Thread.Sleep(rnd.Next(maxWait));

                    //Save each image to disk.
                    await DownloadRemoteImageFile(photo.PhotoUrl1280, imagePath);

                    consoleInfo = "Downloading image " + imageNbr + " of 50: " + post.ImageName(photo.PhotoUrl1280);
                    StatusMsg(string.Format("{0}", consoleInfo));
                    imageNbr++;
                    nbrPhotosInPost += 1;
                }

                if (nbrPhotosInPost > 0)
                    continue;

                imagePath = folderPath + "\\" + post.ImageName(post.PhotoUrl1280);

                if (File.Exists(imagePath))
                {
                    var consoleInfoExist = "Image " + imageNbr + " of 50: " + post.ImageName(post.PhotoUrl1280) + " exists on disk   ";
                    //Random sleep, possible to read the text!
                    Thread.Sleep(rnd.Next(maxWait / 5));
                    StatusMsg(string.Format("{0}", consoleInfoExist));
                    imageNbr++;
                    continue;
                }

                //Random sleep, be nice to the host!
                Thread.Sleep(rnd.Next(maxWait));

                //Save each image to disk.
                DownloadRemoteImageFile(post.PhotoUrl1280, imagePath);

                consoleInfo = "Downloading image " + imageNbr + " of 50: " + post.ImageName(post.PhotoUrl1280);
                StatusMsg(string.Format("{0}", consoleInfo));
                imageNbr++;
            }
        }

        /// <summary>
        /// Download an image from url. 
        /// 
        /// Modified code from Stackoverflow. 
        /// http://stackoverflow.com/questions/3615800/download-image-from-the-site-in-net-c/12631127#12631127
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<bool> DownloadRemoteImageFile(string uri, string fileName)
        {

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (Stream contentStream = await(await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
            {
                await contentStream.CopyToAsync(stream);
                return true;
            }
        }

        public void StatusMsg(string msg)
        {
            StatusMessageEventArgs args = new StatusMessageEventArgs();
            args.Message = msg;
            OnStatusMessage(args);
        }

        protected virtual void OnStatusMessage(StatusMessageEventArgs e)
        {
            DownloadStatusMessage?.Invoke(this, e);
        }

        public event StatusMessageEventHandler DownloadStatusMessage;

    }

    public class StatusMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public delegate void StatusMessageEventHandler(object sender, StatusMessageEventArgs e);
}
