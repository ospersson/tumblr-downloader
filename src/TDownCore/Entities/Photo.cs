using Newtonsoft.Json;

namespace TDownCore
{
    /// <summary>
    /// Maps to a photo from Tumblr API V1
    /// </summary>
    public class Photo
    {
        /// <summary>
        /// Offset is the order of pictures in a post. 
        /// The first is o1 and the second is o2.
        /// </summary>
        [JsonProperty("offset")]
        public string Offset { get; set; }
        [JsonProperty("caption")]
        public string Caption { get; set; }
        [JsonProperty("width")]
        public string Width { get; set; }
        [JsonProperty("height")]
        public string Height { get; set; }
        [JsonProperty("photo-url-1280")]
        public string PhotoUrl1280 { get; set; }
        [JsonProperty("photo-url-500")]
        public string PhotoUrl500 { get; set; }
        [JsonProperty("photo-url-400")]
        public string PhotoUrl400 { get; set; }

        public string ImageName(string photoUrl)
        {
            int idx = photoUrl.LastIndexOf('/');
            return photoUrl.Substring(idx + 1);
        }
    }
}
