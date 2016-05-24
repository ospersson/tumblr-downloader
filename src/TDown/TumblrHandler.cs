﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TDown
{
    public interface ITumblrHandler
    {
        TumblerSiteInfo GetSiteInfo(JObject tumblrJObject);
        IList<Post> GetPostList(JObject tumblrJObject);
        string CreateDownloadUrl(string baseUrl, int startPost, int numberOfPosts);
        JObject GetTumblrObject(string baseDomainUrl, string jsonString, string folderPath);
        string GetBaseDomainFromUrl(string url);
    }
    
    public class TumblrHandler : ITumblrHandler
    {
        public TumblerSiteInfo GetSiteInfo(JObject tumblrJObject)
        {
            //Get JSON results post-total
            JToken siteInfoResult = tumblrJObject["tumblelog"];
            var siteInfo = JsonConvert.DeserializeObject<TumblerSiteInfo>(siteInfoResult.ToString());

            JToken tumblrRoot = tumblrJObject;
            siteInfo.PostsTotal = tumblrRoot.Value<int?>("posts-total") ?? 0;
            return siteInfo;
        }

        public IList<Post> GetPostList(JObject tumblrJObject)
        {
            //Get JSON result objects(the posts) into a list
            IList<JToken> results = tumblrJObject["posts"].Children().ToList();

            // serialize JSON results into .NET objects
            IList<Post> posts = new List<Post>();
            foreach (JToken result in results)
            {
                Post post = JsonConvert.DeserializeObject<Post>(result.ToString());
                posts.Add(post);
            }

            return posts;
        }

        public string CreateDownloadUrl(string baseUrl, int startPost = 0, int numberOfPosts = 20)
        {
            if (baseUrl == string.Empty)
            {
                throw new ApplicationException("CreateDownloadUrl: baseUrl is empty!");
            }

            //Create url 
            string url = @"https://" + baseUrl + "/api/read/json?start=" + startPost;
            url = url + "&num=" + numberOfPosts + "&type=photo";
            return url;
        }

        public JObject GetTumblrObject(string baseDomainUrl, string jsonString, string folderPath)
        {
            if (jsonString == string.Empty)
                throw new ApplicationException("GetTumblrObject: json string is empty");

            JObject tumblrJObject;
            try
            {
                tumblrJObject = JObject.Parse(jsonString);
            }
            catch (Exception)
            {
                IJsonHandler jsonhandler = new JsonHandler();
                jsonhandler.WriteJsonToDebugFile(baseDomainUrl, jsonString, folderPath);
                throw;
            }

            return tumblrJObject;
        }

        public string GetBaseDomainFromUrl(string url)
        {
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);
            url = url.Replace("http", string.Empty);
            url = url.Replace("https", string.Empty);
            
            return url;
        }
    }
}