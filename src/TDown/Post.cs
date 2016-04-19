using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TDown
{
    /// <summary>
    /// Maps to a post from Tumblr API V1
    /// </summary>
    public class Post
    {
        [JsonProperty("id")]
        public Int64 Id { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("url-with-slug")]
        public string UrlWithSlug { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("photo-caption")]
        public string PhotoCaption { get; set; }
        [JsonProperty("photo-link-url")]
        public string PhotoLinkUrl { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("photo-url-1280")]
        public string PhotoUrl1280 { get; set; }
        [JsonProperty("photo-url-500")]
        public string PhotoUrl500 { get; set; }
        [JsonProperty("photo-url-100")]
        public string PhotoUrl100 { get; set; }
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        public string ImageName()
        {
            int idx = PhotoUrl1280.LastIndexOf('/');
            return PhotoUrl1280.Substring(idx + 1); 
        }
    }
}
