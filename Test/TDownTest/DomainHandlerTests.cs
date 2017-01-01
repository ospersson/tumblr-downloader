using Xunit;
using TDown;

namespace TDownTests
{
    public class DomanHandlerTests
    {
        [Fact]
        public void RemoveHTTPFromDomain_withhttpandlastSlash_http_removed()
        {
            //Arrange
            string domain = "http://bestcatpictures.tumblr.com/";
            //Act
            string cleanDomain = DomainHandler.GetBaseDomainFromUrl(domain);

            //Assert
            Assert.Equal("bestcatpictures.tumblr.com", cleanDomain);
        }

        [Fact]
        public void RemoveHTTPFromDomain_withhttp_http_removed()
        {
            //Arrange
            string domain = "http://bestcatpictures.tumblr.com";
            //Act
            string cleanDomain = DomainHandler.GetBaseDomainFromUrl(domain);

            //Assert
            Assert.Equal("bestcatpictures.tumblr.com", cleanDomain);
        }

        [Fact]
        public void RemoveHTTPSFromDomain_withhttpsandlastSlash_https_removed()
        {
            //Arrange
            string domain = "https://bestcatpictures.tumblr.com/";
            //Act
            string cleanDomain = DomainHandler.GetBaseDomainFromUrl(domain);

            //Assert
            Assert.Equal("bestcatpictures.tumblr.com", cleanDomain);
        }
    }
}
