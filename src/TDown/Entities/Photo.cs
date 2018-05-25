using Newtonsoft.Json;

namespace TDown
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
        //Url for raw image resolution. Not all images have those. 
        public string PhotoUrlRaw
        {
            get
            {
                if(PhotoUrl1280.LastIndexOf(".gif") > 0)
                {
                    //It's a gif.
                    return PhotoUrl1280;
                }

                return CreateRawImagePath(PhotoUrl1280);
            }
        }

        internal string CreateRawImagePath(string newUrl)
        {
            const string rawBaseUrl = "https://s3.amazonaws.com/data.tumblr.com/";
            newUrl = newUrl
                .Replace("1280.jpg", "raw.jpg")
                .Replace("500.jpg", "raw.jpg");

            var firstSlash = newUrl.IndexOf(".com/");
            var lastPath = newUrl.Remove(0, firstSlash + 5);

            return rawBaseUrl + lastPath;
        }

        public string ImageName(string photoUrl)
        {
            int idx = photoUrl.LastIndexOf('/');
            return photoUrl.Substring(idx + 1);
        }
    }
}
