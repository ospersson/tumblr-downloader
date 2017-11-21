namespace TDownCore
{
    public class DomainHandler
    {
        /// <summary>
        /// Removes the http(s) with or without //
        /// Clean out the last / from the domain name.
        /// </summary>
        /// <returns>A clean base domainname</returns>
        public static string GetBaseDomainFromUrl(string url)
        {
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);
            url = url.Replace("http", string.Empty);
            url = url.Replace("https", string.Empty);

            int idx = url.LastIndexOf('/');

            if (idx < 13)
                return url;

            return url.Substring(0, idx);
        }
    }
}
