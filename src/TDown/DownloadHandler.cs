using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace TDown
{
    public interface IDownloadHandler
    {
        void DownloadAvatar(string siteName, string folderPath);
        void DownloadAllImages(IList<Post> posts, string folderPath, int maxWait = 5000, bool downloadRaw = false);
        event StatusMessageEventHandler DownloadStatusMessage;
    }

    public class DownloadHandler : IDownloadHandler
    {
        /// <summary>
        /// Download avatar from Tumblr blog. Using open method from Tumblr API V2.
        /// </summary>
        /// <param name="siteName">Name of Tumblr blog</param>
        /// <param name="folderPath">Save avatar to this path</param>
        public void DownloadAvatar(string siteName, string folderPath)
        {
            string avatarUrl = @"https://api.tumblr.com/v2/blog/" + siteName + "/avatar/512";

            var avatarDiskPath = folderPath + "\\Avatar_" + siteName + ".jpg";

            if (File.Exists(avatarDiskPath)) return;

            DownloadRemoteImageFile(avatarUrl, avatarDiskPath);
        }

        public void DownloadAllImages(IList<Post> posts, string folderPath, int maxWait = 10000, bool downloadRaw = false)
        {
            var imageNbr = 1;
            var rnd = new Random();

            foreach (var post in posts)
            {
                string consoleInfo = string.Empty;
                string imagePath = string.Empty;
                string imageName = string.Empty;

                var nbrPhotosInPost = 0;

                //Check if it is a multiple image post.
                foreach (var photo in post.Photos)
                {
                    imageName = ImageName(post, downloadRaw);
                    imagePath = folderPath + "\\" + imageName;

                    if (CheckIfFileExistInStorage(imagePath, imageName, imageNbr, maxWait))
                    {
                        //Image exist in file storage, continue.
                        continue;
                    }

                    //Random sleep, be nice to the host!
                    Thread.Sleep(rnd.Next(maxWait));

                    bool HasDownloadedRawPhoto = false;

                    //Save each image to disk.
                    if (downloadRaw)
                    {
                        //Try to download raw image.
                        HasDownloadedRawPhoto = DownloadRemoteImageFile(photo.PhotoUrlRaw, imagePath);
                    }

                    if (HasDownloadedRawPhoto == false)
                    {
                        //Raw download was not performed or it was a failure use 1280px fallback.
                        DownloadRemoteImageFile(photo.PhotoUrl1280, imagePath);
                    }

                    consoleInfo = "Downloading image " + imageNbr + " of 50: " + imageName;

                    StatusMsg(string.Format("{0}", consoleInfo));
                    imageNbr++;
                    nbrPhotosInPost += 1;
                }   
                    
                if (nbrPhotosInPost > 0)
                    continue;

                imageName = ImageName(post, downloadRaw);
                imagePath = folderPath + "\\" + imageName;

                if (CheckIfFileExistInStorage(imagePath, imageName, imageNbr, maxWait))
                {
                    //Image exist in file storage, continue.
                    continue;
                }

                //Random sleep, be nice to the host!
                Thread.Sleep(rnd.Next(maxWait));

                bool HasDownloadedRaw = false;

                //Save each image to disk.
                if (downloadRaw)
                {
                    //Try to download raw image.
                    HasDownloadedRaw = DownloadRemoteImageFile(post.PhotoUrlRaw, imagePath);
                }

                if (HasDownloadedRaw == false)
                {
                    //Raw download was not performed or it was a failure use 1280px fallback.
                    DownloadRemoteImageFile(post.PhotoUrl1280, imagePath);
                }

                consoleInfo = "Downloading image " + imageNbr + " of 50: " + imageName;
                StatusMsg(string.Format("{0}", consoleInfo));
                imageNbr++;
            }
        }

        private bool CheckIfFileExistInStorage(string imagePath, string imageName, int imageNumber, int maxWait)
        {
            if (!File.Exists(imagePath))
                return false;
            
            var rnd = new Random();
            var consoleInfoExist = "Image " + imageNumber + " of 50: " + imageName + " exists on disk   ";
            //Random sleep, possible to read the text!
            Thread.Sleep(rnd.Next(maxWait / 5));
            StatusMsg(string.Format("{0}", consoleInfoExist));
            imageNumber++;            

            return true;
        }

        private string ImageName(Post post, bool doDownloadRaw)
        {
            string imageName = string.Empty;

            if(doDownloadRaw)
            {
                imageName = post.ImageName(post.PhotoUrlRaw);
            }
            else
            {
                imageName = post.ImageName(post.PhotoUrl1280);
            }

            return imageName;
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
        private bool DownloadRemoteImageFile(string uri, string fileName)
        {
            var whcollection = new WebHeaderCollection();
            whcollection.Set("Love-You-Guys", "Downloading some nice pictures! Thanks!!");

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.UserAgent = "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10.8.2; en-US) tdown/1.0.0 Sunflower/1.61803398";
            request.Headers = whcollection;

            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException we)
            {
                Console.WriteLine(we.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return SaveWebResponesToDisk(response, "image", fileName);
        }

        private bool SaveWebResponesToDisk(HttpWebResponse response, string contentTypeStartsWith, string fileName)
        {
            if (response == null)
                throw new Exception("HttpWebResponse response is null");

            if(string.IsNullOrEmpty(contentTypeStartsWith))
                throw new Exception("contentTypeStartsWith is null or empty");

            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine(" Method:SaveWebResponesToDisk: fileName is null or empty");
                return false;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith(contentTypeStartsWith, StringComparison.OrdinalIgnoreCase))
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
            StatusMessageEventArgs args = new StatusMessageEventArgs
            {
                Message = msg
            };

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
