using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TDownCore;

namespace TDownConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
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

#if DEBUG
            baseDomainUrl = "bestcatpictures.tumblr.com";
            doWriteJson = true;
#endif

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
            string jsonString = await DownloadJson(baseDiskPath, baseDomainUrl, doWriteJson, url, jsonHandler);

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
            downloadHandler.DownloadAllImages(posts, folderPath);
            Console.WriteLine("\nFirst batch done.");

            nbrOfPostsFetchedFromUrl += 50;
            MainDownloadLoop(baseDiskPath, baseDomainUrl, nbrOfPostPerCall, doWriteJson, folderPath, jsonHandler, tumblrHandler, url, downloadHandler, siteInfo);

            Console.WriteLine("\nDownload done!");
            Console.WriteLine("\nPress return to exit.");
            Console.ReadLine();
        }

        private static async void MainDownloadLoop(string baseDiskPath, string baseDomainUrl, int nbrOfPostPerCall, bool doWriteJson, string folderPath, IJsonHandler jsonHandler, ITumblrHandler tumblrHandler, string url, IDownloadHandler downloadHandler, TumblerSiteInfo siteInfo)
        {
            //Main download loop.
            for (int currentPost = 50; currentPost < siteInfo.PostsTotal; currentPost += 50)
            {
                //Get new url
                url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, currentPost, nbrOfPostPerCall);

                //Download json from url
                var jsonString = await DownloadJson(baseDomainUrl, baseDiskPath, doWriteJson, url, jsonHandler);

                Console.WriteLine("\nParsing JSON for batch {0} to {1}: ", currentPost, (currentPost + 50));
                var tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString.ToString(), folderPath);

                //Get a list of tumblr images post(s)
                var posts = tumblrHandler.GetPostList(tumblrJObject);

                //Download images from the tumblr image post(s)
                downloadHandler.DownloadAllImages(posts, folderPath);
            }
        }

        private static async Task<string> DownloadJson(string baseDiskPath, string baseDomainUrl, bool doWriteJson, string url, IJsonHandler jsonhandler)
        {
            string jsonString = string.Empty;
            using (var webClient = new HttpClient())
            {
                jsonString = await jsonhandler.DownloadJson(webClient, url, doWriteJson, baseDiskPath, baseDomainUrl);
            }

            return jsonString;
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
