using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
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
            bool downloadRaw = false;

            //Read from console
#if DEBUG
            char[] delimiterChars = { ' ' };
            var commandLine = Console.ReadLine();
            string[] args2 = commandLine.Split(delimiterChars);
            doWriteJson = true;
#else
            string[] args2 = Environment.GetCommandLineArgs();
#endif
            var consoleParser = new ConsoleParser();
            consoleParser.Parse(args2, out baseDomainUrl, out baseDiskPath, out downloadRaw, out doWriteJson);

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

            Console.WriteLine("domain: " + baseDomainUrl + " diskpath: " + baseDiskPath + " downloadraw: " + downloadRaw);
            InitEvents();

            Console.WriteLine("Starting download from {0} to: {1} ", baseDomainUrl, baseDiskPath);

            baseDomainUrl = DomainHandler.GetBaseDomainFromUrl(baseDomainUrl);
            
            //Create download folder(if not exist).
            var folderPath = baseDiskPath + "\\" + baseDomainUrl;
            Directory.CreateDirectory(folderPath);

            IJsonLogger jsonLogger = new JsonLogger(folderPath, baseDomainUrl);
            IJsonHandler jsonHandler = new JsonHandler(jsonLogger);
            ITumblrHandler tumblrHandler = new TumblrHandler(jsonHandler);

            var url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, 0, nbrOfPostPerCall);

            IDownloadHandler downloadHandler = new DownloadHandler();
            downloadHandler.DownloadAvatar(baseDomainUrl, folderPath);
            downloadHandler.DownloadStatusMessage += StatusMessage;

            //Download json from url
            var jsonString = jsonHandler.DownloadJson(url, doWriteJson, baseDiskPath, baseDomainUrl);

            //JObject tumblrJObject;
            TumblerSiteInfo siteInfo;
            IList<Post> posts = new List<Post>();

            try
            {
                Console.WriteLine("Parsing JSON");
                var tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString.ToString(), folderPath);
                siteInfo = tumblrHandler.GetSiteInfo(tumblrJObject);
                posts = tumblrHandler.GetPostList(tumblrJObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n --- Writing stack trace: ---");
                Console.WriteLine("\n" + ex.ToString());
                Console.WriteLine("\nPress enter to exit");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Downloading the latest 50 images of {0}", siteInfo.PostsTotal);
            downloadHandler.DownloadAllImages(posts, folderPath, 5000, downloadRaw);
            Console.WriteLine("\nFirst batch done.");

            nbrOfPostsFetchedFromUrl += 50;
            MainDownloadLoop(baseDiskPath, baseDomainUrl, nbrOfPostPerCall, doWriteJson, folderPath, ref url, ref jsonString, siteInfo, ref posts, downloadRaw);

            Console.WriteLine("\nDownload done!");
            Console.WriteLine("\nPress return to exit.");
            Console.ReadLine();
        }

        private static void MainDownloadLoop(
            string baseDiskPath, 
            string baseDomainUrl, 
            int nbrOfPostPerCall, 
            bool doWriteJson, 
            string folderPath,   
            ref string url, 
            ref string jsonString, 
            TumblerSiteInfo siteInfo, 
            ref IList<Post> posts,
            bool downloadRaw)
        {
            IJsonLogger jsonLogger = new JsonLogger(folderPath, baseDomainUrl);
            IJsonHandler jsonHandler = new JsonHandler(jsonLogger);
            ITumblrHandler tumblrHandler = new TumblrHandler(jsonHandler);
            IDownloadHandler downloadHandler = new DownloadHandler();

            //Main download loop.
            for (int currentPost = 50; currentPost < siteInfo.PostsTotal; currentPost += 50)
            {
                //Get new url
                url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, currentPost, nbrOfPostPerCall);

                //Download json from url
                jsonString = jsonHandler.DownloadJson(url, doWriteJson, baseDiskPath, baseDomainUrl);

                Console.WriteLine("\nParsing JSON for batch {0} to {1}: ", currentPost, (currentPost + 50));

                JObject tumblrJObject;

                try
                {
                    tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString.ToString(), folderPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\n --- Writing stack trace: ---");
                    Console.WriteLine("\n" + ex.ToString());
                    continue;
                }

                //Get a list of tumblr images post(s)
                posts = tumblrHandler.GetPostList(tumblrJObject);

                //Get all status messages events from downloadhandler
                downloadHandler.DownloadStatusMessage += StatusMessage;

                //Download images from the tumblr image post(s)
                downloadHandler.DownloadAllImages(posts, folderPath, 5000, downloadRaw);      
            }
        }

        private static void InitEvents()
        {
            //Handles the Ctrl-C event.
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                ExitApp();
            };
        }

        private static void ExitApp()
        {
            Console.WriteLine("\nCancel key pressed, console app is closing.");
            //Call exit
            Environment.Exit(-1);
        }

        private static void StatusMessage(object sender, StatusMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
