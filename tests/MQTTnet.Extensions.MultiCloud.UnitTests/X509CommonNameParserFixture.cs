using MQTTnet.Extensions.MultiCloud.Connections;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class X509CommonNameParserFixture
    {
        [Fact]
        public void ParseCNFromSimple()
        {
            var cert = new X509Certificate2("onething.pfx", "1234");
            var parsed = X509CommonNameParser.GetCNFromCertSubject(cert);
            Assert.Equal("onething", parsed);
        }
        [Fact]
        public void ParseCNFromFullDN()
        {
            var cert = new X509Certificate2("client.pfx", "1234");
            var parsed = X509CommonNameParser.GetCNFromCertSubject(cert);
            Assert.Equal("client", parsed);
        }
    }
}
