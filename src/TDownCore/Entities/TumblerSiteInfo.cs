using Newtonsoft.Json;

namespace TDownCore
{
    /// <summary>
    /// Maps to site info from Tumblr API V1.
    /// </summary>
    public class TumblerSiteInfo
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
        [JsonProperty("cname")]
        public string Cname { get; set; }
        [JsonProperty("posts-start")]
        public int PostsStart { get; set; }
        [JsonProperty("posts-total")]
        public int PostsTotal { get; set; }
    }
}
