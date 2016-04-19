using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using TDown;

namespace TDownConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string baseDiskPath = string.Empty;
            string baseDomainUrl = string.Empty;
            int nbrOfPostsFetchedFromUrl = 0;
            int nbrOfPostPerCall = 50;
            bool doWriteJson = false;

            //Read from console
            string[] args2 = Environment.GetCommandLineArgs();

            if (args2.Length > 1)
            {
                baseDomainUrl = args2[1];
            }
            if (args2.Length > 2)
            {
                baseDiskPath = args2[2];
            }
            if (args2.Length > 3)
            {
                if (args2[3] == "-l")
                    doWriteJson = true;
            }

            if (baseDomainUrl == string.Empty)
            {
                Console.WriteLine("\nPlease enter the blog where images(eg bestcatpictures.tumblr.com) will be downloaded from!");
                Console.WriteLine("\nPress enter to exit");
                Console.ReadKey();
                return;
            }
            else if (baseDiskPath == string.Empty)
            {
                baseDiskPath = Infrastructure.AssemblyDirectory;
            }

            //Handles the Ctrl-C event.
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                ExitApp();
            };

            Console.WriteLine("Starting download from {0} to: {1} ", baseDomainUrl, baseDiskPath);

            ITumblrHandler tumblrHandler = new TumblrHandler();
            var url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, 0, nbrOfPostPerCall);

            //Create download folder(if not exist).
            var folderPath = baseDiskPath + "\\" + baseDomainUrl;
            Directory.CreateDirectory(folderPath);

            DownloadHandler.DownloadAvatar(baseDomainUrl, folderPath);

            //Download json from url
            IJsonHandler jsonhandler = new JsonHandler();
            var jsonString = jsonhandler.DownloadJson(url, doWriteJson, baseDiskPath, baseDomainUrl);

            JObject tumblrJObject;

            try
            {
                Console.WriteLine("Parsing JSON");
                tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString, folderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n --- Writing stack trace: ---");
                Console.WriteLine("\n" + ex.ToString());
                Console.WriteLine("\nPress enter to exit");
                Console.ReadKey();
                return;
            }
            
            TumblerSiteInfo siteInfo = tumblrHandler.GetSiteInfo(tumblrJObject);

            Console.WriteLine("Downloading the latest 50 images of {0}", siteInfo.PostsTotal);

            IList<Post> posts = tumblrHandler.GetPostList(tumblrJObject);
            DownloadHandler.DownloadAllImages(posts, folderPath);
            Console.WriteLine("\nFirst batch done.");

            nbrOfPostsFetchedFromUrl += 50;

            //Main download loop.
            for (int currentPost = 50; currentPost < siteInfo.PostsTotal; currentPost += 50)
            {
                //Get new url
                url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, currentPost, nbrOfPostPerCall);

                //Download json from url
                jsonString = jsonhandler.DownloadJson(url, doWriteJson, baseDiskPath, baseDomainUrl);

                Console.WriteLine("\nParsing JSON for batch {0} to {1}: ", currentPost, (currentPost + 50));
                tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString, folderPath);

                //Get a list of tumblr images post(s)
                posts = tumblrHandler.GetPostList(tumblrJObject);

                //Download images from the tumblr image post(s)
                DownloadHandler.DownloadAllImages(posts, folderPath);
            }

            Console.WriteLine("\nDownload done!");
            Console.WriteLine("\nPress return to exit.");
            Console.ReadLine();
        }

        private static void ExitApp()
        {
            Console.WriteLine("\nCancel key pressed, console app is closing.");
            //Call exit
            Environment.Exit(-1);
        }  
    }
}
