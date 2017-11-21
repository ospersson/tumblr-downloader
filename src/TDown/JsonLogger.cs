using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDown
{
    public interface IJsonLogger
    {
        void LogJson(string json);
    }

    public class JsonLogger : IJsonLogger
    {
        private string _folderPath;
        private string _domain;

        public JsonLogger(string folderPath, string domain)
        {
            _folderPath = folderPath;
            _domain = domain;
        }

        public void LogJson(string json)
        {
            if (_folderPath == string.Empty)
                throw new ApplicationException("LogJsonToDisk, folder path is empty!");
            else if (_domain == string.Empty)
            {
                throw new ApplicationException("LogJsonToDisk, domain is empty!");
            }
            else if (json == string.Empty)
            {
                throw new ApplicationException("LogJsonToDisk, jsonString is empty!");
            }

            //Log raw json to disk
            var jsonArray = new[] { json };

            if (_folderPath.Contains(_domain))
            {
                _folderPath += "\\JSON";
            }
            else
            {
                _folderPath = _folderPath + "\\" + _domain + "\\JSON";
            }

            Directory.CreateDirectory(_folderPath);

            var nbrFiles = Directory.GetFiles(_folderPath).Length;

            var filePath = _folderPath + "\\" + Guid.NewGuid().ToString() + "_" + _domain + "_" + (nbrFiles + 1) + ".json";
            Console.WriteLine("Saving .json to this path: " + _folderPath);
            File.WriteAllLines(filePath, jsonArray);
        }
    }

    
}
