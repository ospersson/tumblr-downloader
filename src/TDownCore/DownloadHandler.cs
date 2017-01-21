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

            //var whcollection = new WebHeaderCollection();
            //whcollection.Set("Love-You-Guys", "Downloading some nice pictures! Thanks!!");

            //var request = (HttpWebRequest)WebRequest.Create(uri);
            //request.UserAgent = "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10.8.2; en-US) tdown/1.0.0 Sunflower/1.61803398";
            //request.Headers = whcollection;

            //HttpWebResponse response;

            //try
            //{
            //    response = (HttpWebResponse)request.GetResponse();
            //}
            //catch (WebException we)
            //{
            //    Console.WriteLine(we.Message);
            //    return false;
            //}
            //catch (Exception)
            //{
            //    throw;
            //}

            //return SaveWebResponesToDisk(response, "image", fileName);
        }

        //public bool SaveWebResponesToDisk(HttpWebResponse response, string contentTypeStartsWith, string fileName)
        //{
        //    if (response == null)
        //        throw new Exception("HttpWebResponse response is null");

        //    if(string.IsNullOrEmpty(contentTypeStartsWith))
        //        throw new Exception("contentTypeStartsWith is null or empty");

        //    if (string.IsNullOrEmpty(fileName))
        //    {
        //        Console.WriteLine(" Method:SaveWebResponesToDisk: fileName is null or empty");
        //        return false;
        //    }

        //    // Check that the remote file was found. The ContentType
        //    // check is performed since a request for a non-existent
        //    // image file might be redirected to a 404-page, which would
        //    // yield the StatusCode "OK", even though the image was not
        //    // found.
        //    if ((response.StatusCode == HttpStatusCode.OK ||
        //        response.StatusCode == HttpStatusCode.Moved ||
        //        response.StatusCode == HttpStatusCode.Redirect) &&
        //        response.ContentType.StartsWith(contentTypeStartsWith, StringComparison.OrdinalIgnoreCase))
        //    {

        //        // if the remote file was found, download it
        //        using (Stream inputStream = response.GetResponseStream())
        //        using (Stream outputStream = File.OpenWrite(fileName))
        //        {
        //            byte[] buffer = new byte[4096];
        //            int bytesRead;
        //            do
        //            {
        //                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
        //                outputStream.Write(buffer, 0, bytesRead);
        //            } while (bytesRead != 0);
        //        }
        //        return true;
        //    }
        //    else
        //        return false;
        //}

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
