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
                .Replace("<a href=\\'", string.Empty)
                .Replace("' class", string.Empty)
                .Replace("' tumblr", string.Empty)
                .Replace("\\=", string.Empty)
                .Replace("\\'", string.Empty)
                .Replace("i's", string.Empty)
                .Replace(">'L", string.Empty)
                .Replace("u're", "u re")
                .Replace("Je t'", "Je t")
                .Replace("n't", "nt")
                .Replace("t's", "");

            var json = CleanValueFromSingleQuote("reblogged-from-title", jsonString.ToString());
            json = CleanValueFromSingleQuote("reblogged-root-title", jsonString.ToString());

            return json;
        }

        private string CleanValueFromSingleQuote(string value, string json)
        {
            int n = 0;

            while ((n = json.IndexOf(value, n, StringComparison.CurrentCultureIgnoreCase)) != -1)
            {
                n += value.Length;

                string sub1 = json.Substring(n, 200).Trim();

                int indexMarker = sub1.IndexOf("','reblog");

                if (indexMarker == -1)
                {
                    //Try this pattern
                    indexMarker = sub1.IndexOf("',\r\n\t\t'");
                }

                if (indexMarker != -1 && indexMarker > 4)
                {
                    string subtoclean = sub1.Substring(4, (indexMarker - 4));
                    string cleanedSub = subtoclean.Replace("'", string.Empty);

                    json = json.Replace(subtoclean, cleanedSub);
                }
            }

            return json;
        }

    }
}
