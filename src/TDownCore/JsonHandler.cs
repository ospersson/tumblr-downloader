using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TDownCore
{
    public interface IJsonHandler
    {
        void WriteJsonToDebugFile(string baseUrl, string jsonString, string folderPath);
        Task<string> DownloadJson(HttpClient webClient, string url, bool doLogJson = false, string folderPath = "", string domain = "");
    }

    public class JsonHandler : IJsonHandler
    {
        private IJsonLogger _jsonLogger;

        public JsonHandler(IJsonLogger jsonLogger)
        {
            _jsonLogger = jsonLogger;
        }

        /// <summary>
        /// Write the JSON file to disk for debug/ocular inspection.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="jsonString"></param>
        /// <param name="folderPath"></param>
        public void WriteJsonToDebugFile(string baseUrl, string jsonString, string folderPath)
        {
            //Write json file to disk for debug.
            var pathAndName = @"" + folderPath + "\\" + baseUrl + ".txt";
            File.WriteAllText(pathAndName, jsonString);
        }

        public async Task<string> DownloadJson(HttpClient httpClient, string url, bool doLogJson = false, string folderPath = "", string domain = "")
        {
            if (url == string.Empty)
                throw new Exception("url is empty!");

            if (httpClient == null)
                throw new Exception("webClient is null!");

            string jsonString = string.Empty;

            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var uri = new Uri(url);
                jsonString = await httpClient.GetStringAsync(uri);
                if (doLogJson)
                {
                    _jsonLogger.LogJson(jsonString);
                }

            }
            catch (Exception we)
            {
                Console.WriteLine(we.Message);
            }

            //Return a clean json string.
            return CleanJson(jsonString);
        }

        /// <summary>
        /// Method for cleaning the result before parsing the string into JSON.
        /// Todo://This is not good. Find better way of cleaning.
        /// </summary>
        /// <param name="jsonStringTemp">String to clean</param>
        /// <returns>Clean string</returns>
        public string CleanJson(string jsonStringTemp)
        {
            if (jsonStringTemp == string.Empty)
                throw new Exception("CleanJson, json input string is empty!");

            var jsonString = new StringBuilder();

            jsonStringTemp = jsonStringTemp.Replace("var tumblr_api_read = ", string.Empty);
            jsonStringTemp = jsonStringTemp.Replace("\"", "'");
            jsonStringTemp = jsonStringTemp.Replace(";", string.Empty);

            //AOB
            int first = jsonStringTemp.IndexOf("<script");
            int last = jsonStringTemp.IndexOf("script>");

            if (first > 0)
            {
                string sub1 = jsonStringTemp.Substring(0, first);
                var infoLength = jsonStringTemp.Length - (last + 7);
                string sub2 = jsonStringTemp.Substring(last + 7, infoLength);

                jsonString.Append(sub1);
                jsonString.Append(sub2);
            }
            else
            {
                jsonString.Append(jsonStringTemp);
            }

            jsonString.Replace("<p>", string.Empty)
                .Replace("</p>", string.Empty)
                .Replace("<a href=\\'", string.Empty)
                .Replace("' class", string.Empty)
                .Replace("' tumblr", string.Empty)
                .Replace("\\=", string.Empty)
                .Replace("\\'", string.Empty);

            var json = CleanValueFromValue("reblogged-from-title", jsonString.ToString(), "','reblog");
            json = CleanValueFromValue("reblogged-root-title", json, "','reblog");
            json = CleanValueFromValue("photo-caption", json, "','width");

            return json;
        }

        /// <summary>
        /// This function tries to clean out values from certain json key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="json"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        private string CleanValueFromValue(string key, string json, string marker)
        {
            int n = 0;

            while ((n = json.IndexOf(key, n, StringComparison.CurrentCulture)) != -1)
            {
                n += key.Length;

                var maxlen = json.Length - n;
                if ((json.Length - n) > 2800)
                {
                    maxlen = 2800;
                }

                string sub1 = json.Substring(n, maxlen).Trim();

                int indexMarker = sub1.IndexOf(marker);

                if (indexMarker == -1)
                {
                    //Try this pattern
                    indexMarker = sub1.IndexOf("',\r\n\t\t'");
                }
                else if (indexMarker != -1 && indexMarker > 3)
                {
                    string subtoclean = string.Empty;
                    sub1 = sub1.Trim();
                    subtoclean = sub1.Substring(3, indexMarker - 3);

                    if (subtoclean.Length > 2500)
                    {
                        if (subtoclean.Substring(subtoclean.Length - 6).Contains("'"))
                        {
                            subtoclean = subtoclean.Replace("',", string.Empty);
                        }
                    }

                    json = json.Replace(subtoclean, string.Empty);
                }
            }

            return json;
        }

    }
}
