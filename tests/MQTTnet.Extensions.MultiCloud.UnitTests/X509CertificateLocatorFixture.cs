using MQTTnet.Extensions.MultiCloud.Connections;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class X509CertificateLocatorFixture
    {
        [Fact]
        public void ParseCertFromPFX()
        {
            string certSettings = "onething.pfx|1234";
            var cert = X509ClientCertificateLocator.Load(certSettings);
            Assert.NotNull(cert);
            Assert.Equal("CN=onething", cert.SubjectName.Name);
            Assert.Equal("8E983707D3F802E6717BBCD193129946573F31D4", cert.Thumbprint);
        }

        [Fact]
        public void LoadCertFromStore()
        {
            var testCert = X509ClientCertificateLocator.Load("onething.pfx|1234");
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(testCert);

            string certSettings = "8E983707D3F802E6717BBCD193129946573F31D4";
            var cert = X509ClientCertificateLocator.Load(certSettings);
            Assert.NotNull(cert);
            Assert.Equal("CN=onething", cert.SubjectName.Name);
            Assert.Equal("8E983707D3F802E6717BBCD193129946573F31D4", cert.Thumbprint);

            store.Remove(testCert);
            store.Close();
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void ParseCertFromPEM()
        {
            string certSettings = "onething.pem|onething.key";
            var cert = X509ClientCertificateLocator.Load(certSettings);
            Assert.NotNull(cert);
            Assert.Equal("CN=onething", cert.SubjectName.Name);
            Assert.Equal("8E983707D3F802E6717BBCD193129946573F31D4", cert.Thumbprint);
        }

        [Fact]
        public void ParseCertFromPEMWithKey()
        {
            string certSettings = "onething.pem|onething.priv.key|1234";
            var cert = X509ClientCertificateLocator.Load(certSettings);
            Assert.NotNull(cert);
            Assert.Equal("CN=onething", cert.SubjectName.Name);
            Assert.Equal("8E983707D3F802E6717BBCD193129946573F31D4", cert.Thumbprint);
        }
#endif

    }
}
