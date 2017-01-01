using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using TDown;

namespace TDownGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILogger logg;
        bool isDownloadStarted = false;
        bool IsCancellationRequested = false;

        public MainWindow()
        {
            InitializeComponent();
            logg = new Logger();
            this.DataContext = logg;
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            string baseDiskPath = string.Empty;
            string baseDomainUrl = string.Empty;
            baseDomainUrl = DomainHandler.GetBaseDomainFromUrl(txtSite.Text);
            baseDiskPath = txtDownloadFolder.Text;

            if(isDownloadStarted)
            {
                isDownloadStarted = false;
                IsCancellationRequested = true;
                logg.LogText = "Download cancelled. Current batch will continue until done.";
                btnDownload.Content = "Download";
                btnDownload.Background = Brushes.LightGreen;
                return;
            }
            else
            {
                isDownloadStarted = true;
                btnDownload.Content = "Stop";
                btnDownload.Background = Brushes.Red;
            }

            bool doCreateSubFolder = chkSubfolder.IsChecked ?? false;

            await Task.Factory.StartNew(() =>
            {
                TumblrWorker(logg, baseDomainUrl, baseDiskPath, doCreateSubFolder);
            });
        }

        public void TumblrWorker(ILogger logger, string baseDomainUrl, string baseDiskPath, bool doCreateSubFolder)
        {
            int nbrOfPostPerCall = 50;
            bool doWriteJson = false;
            string folderPath = string.Empty;

            if (baseDomainUrl == string.Empty)
            {
                logger.LogText = "Please enter the blog where images(eg bestcatpictures.tumblr.com) will be downloaded from!";
                return;
            }
            else if (baseDiskPath == string.Empty)
            {
                baseDiskPath = Infrastructure.AssemblyDirectory;
            }

            logger.LogText = string.Format("Starting download from {0} to: {1} ", baseDomainUrl, baseDiskPath);

            IJsonLogger jsonLogger = new JsonLogger(folderPath, baseDomainUrl);
            IJsonHandler jsonHandler = new JsonHandler(jsonLogger);
            ITumblrHandler tumblrHandler = new TumblrHandler(jsonHandler);
            baseDomainUrl = DomainHandler.GetBaseDomainFromUrl(baseDomainUrl);
            var url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, 0, nbrOfPostPerCall);

            if (doCreateSubFolder)
            {
                folderPath = baseDiskPath + "\\" + baseDomainUrl;
            }
            else
            {
                folderPath = baseDiskPath;
            }

            //Create download folder(if not exist).   
            Directory.CreateDirectory(folderPath);

            IDownloadHandler downloadHandler = new DownloadHandler();
            downloadHandler.DownloadAvatar(baseDomainUrl, folderPath);
            downloadHandler.DownloadStatusMessage += StatusMessage;

            //Download json from url
            string jsonString = DownloadJson(baseDomainUrl, baseDiskPath, doWriteJson, url, jsonHandler);

            JObject tumblrJObject;

            try
            {
                logger.LogText = "Parsing JSON";
                tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString, folderPath);
            }
            catch (Exception ex)
            {
                logger.LogText = "--- Writing stack trace: ---";
                logger.LogText = ex.ToString();
                return;
            }

            TumblerSiteInfo siteInfo = tumblrHandler.GetSiteInfo(tumblrJObject);

            logger.LogText = string.Format("Downloading the latest 50 images of {0}", siteInfo.PostsTotal);

            IList<Post> posts = tumblrHandler.GetPostList(tumblrJObject);
            downloadHandler.DownloadAllImages(posts, folderPath);
            logger.LogText = "First batch done.";

            MainDownloadLoop(logger, siteInfo.PostsTotal, baseDomainUrl, baseDiskPath, folderPath, nbrOfPostPerCall, doWriteJson);

            logger.LogText = "Download done!";
        }

        private string DownloadJson(string baseDomainUrl, string baseDiskPath, bool doWriteJson, string url, IJsonHandler jsonhandler)
        {
            string jsonString = string.Empty;
            using (var webClient = new WebClient())
            {
                jsonString = jsonhandler.DownloadJson(webClient, url, doWriteJson, baseDiskPath, baseDomainUrl);
            }

            return jsonString;
        }

        private void MainDownloadLoop(ILogger logger, int postsTotal, string baseDomainUrl, string baseDiskPath, string folderPath, int nbrOfPostPerCall, bool doWriteJson = false)
        {
            int nbrOfPostsFetchedFromUrl = 0;
            string url = string.Empty;

            IJsonLogger jsonLogger = new JsonLogger(folderPath, baseDomainUrl);
            IJsonHandler jsonhandler = new JsonHandler(jsonLogger);
            ITumblrHandler tumblrHandler = new TumblrHandler(jsonhandler);
            JObject tumblrJObject;

            var downloadHandler = new DownloadHandler();
            downloadHandler.DownloadStatusMessage += StatusMessage;

            nbrOfPostsFetchedFromUrl += nbrOfPostPerCall;

            //Main download loop.
            for (int currentPost = 50; currentPost < postsTotal; currentPost += 50)
            {
                //Check if thread is cancelled.
                if (IsCancellationRequested) return;

                //Get new url
                url = tumblrHandler.CreateDownloadUrl(baseDomainUrl, currentPost, nbrOfPostPerCall);

                //Download json from url
                string jsonString = DownloadJson(baseDomainUrl, baseDiskPath, doWriteJson, url, jsonhandler);

                logger.LogText = string.Format("\nParsing JSON for batch {0} to {1}: ", currentPost, (currentPost + 50));
                tumblrJObject = tumblrHandler.GetTumblrObject(baseDomainUrl, jsonString, folderPath);

                //Get a list of tumblr images post(s)
                var posts = tumblrHandler.GetPostList(tumblrJObject);

                //Download images from the tumblr image post(s)
                downloadHandler.DownloadAllImages(posts, folderPath);

                nbrOfPostsFetchedFromUrl += nbrOfPostPerCall;
            }
        }

        private void StatusMessage(object sender, StatusMessageEventArgs e)
        {
            logg.LogText = e.Message;
        }

        private void btnBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new FolderBrowserDialog();

            folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtDownloadFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnBrowseDownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", @"c:\test");
        }
    }


    public interface ILogger
    {
        string LogText { get; set; }
        event PropertyChangedEventHandler PropertyChanged;
    }

    public class Logger : ILogger, INotifyPropertyChanged
    {
        private static StringBuilder _logText = new StringBuilder();

        public string LogText
        {
            get
            {
                return _logText.ToString();
            }
            set
            {
                _logText.AppendFormat(value + Environment.NewLine);
                OnPropertyChanged("LogText");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
