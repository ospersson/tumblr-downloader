using System;
using System.IO;
using System.Net;
using System.Text;

namespace TDown
{
    public interface IJsonHandler
    {
        void WriteJsonToDebugFile(string baseUrl, string jsonString, string folderPath);
        string DownloadJson(WebClient webClient, string url, bool doLogJson = false, string folderPath = "", string domain = "");
    }

    public class JsonHandler : IJsonHandler
    {
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

        public string DownloadJson(WebClient webClient, string url, bool doLogJson = false, string folderPath = "", string domain = "")
        {
            if (url == string.Empty)
                throw new ApplicationException("url is empty!");
                
            if (webClient == null)
                throw new ApplicationException("webClient is null!");

            string jsonString;

            webClient.Proxy = null;

            try
            {
                var uri = new Uri(url);
                jsonString = webClient.DownloadString(uri);
                if(doLogJson)
                {
                    folderPath = LogJsonToDisk(folderPath, domain, jsonString);
                }

            }
            catch (WebException we)
            {
                Console.WriteLine(we.Message);
                Console.WriteLine("Press return to exit");
                Console.ReadLine();
                return string.Empty;
            }
            catch(Exception)
            {
                throw;
            }

            //Return a clean json string.
            return CleanJson(jsonString);
            
        }

        private static string LogJsonToDisk(string folderPath, string domain, string jsonString)
        {
            if (folderPath == string.Empty)
                throw new ApplicationException("LogJsonToDisk, folder path is empty!");
            else if (domain == string.Empty)
            {
                throw new ApplicationException("LogJsonToDisk, domain is empty!");
            }
            else if (jsonString == string.Empty)
            {
                throw new ApplicationException("LogJsonToDisk, jsonString is empty!");
            }

            //Log raw json to disk
            var jsonArray = new[] { jsonString };

            if (folderPath.Contains(domain))
            {
                folderPath += "\\JSON";
            }
            else
            {
                folderPath = folderPath + "\\" + domain + "\\JSON";
            }

            Directory.CreateDirectory(folderPath);

            var nbrFiles = Directory.GetFiles(folderPath).Length;

            folderPath = folderPath + "\\" + domain + "_" + (nbrFiles + 1) + ".json";
            Console.WriteLine("Saving .json to this path: " + folderPath);
            File.WriteAllLines(folderPath, jsonArray);
            return folderPath;
        }

        /// <summary>
        /// Method for cleaning the result before parsing the string into JSON.
        /// </summary>
        /// <param name="jsonStringTemp">String to clean</param>
        /// <returns>Clean string</returns>
        private static string CleanJson(string jsonStringTemp)
        {
            if (jsonStringTemp == string.Empty)
                throw new ApplicationException("CleanJson, json input string is empty!");

            StringBuilder jsonString = new StringBuilder();

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
                .Replace("Je t'", "Je t")
                .Replace("n't", "nt");
                

            return jsonString.ToString();
        }
    }
}
