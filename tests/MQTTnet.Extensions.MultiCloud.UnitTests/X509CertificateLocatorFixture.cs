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
            Assert.Equal("0B392CC5E58DADEB5A632AB92F29772BF2B2D6BA", cert.Thumbprint);
        }

        [Fact]
        public void LoadCertFromStore()
        {
            var testCert = X509ClientCertificateLocator.Load("onething.pfx|1234");
            X509Store store = new (StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(testCert);

            string certSettings = "0B392CC5E58DADEB5A632AB92F29772BF2B2D6BA";
            var cert = X509ClientCertificateLocator.Load(certSettings);
            Assert.NotNull(cert);
            Assert.Equal("CN=onething", cert.SubjectName.Name);
            Assert.Equal("0B392CC5E58DADEB5A632AB92F29772BF2B2D6BA", cert.Thumbprint);

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
            Assert.Equal("0B392CC5E58DADEB5A632AB92F29772BF2B2D6BA", cert.Thumbprint);
        }

        [Fact]
        public void ParseCertFromPEMWithKey()
        {
            string certSettings = "onething.pem|onething.priv.key|1234";
            var cert = X509ClientCertificateLocator.Load(certSettings);
            Assert.NotNull(cert);
            Assert.Equal("CN=onething", cert.SubjectName.Name);
            Assert.Equal("0B392CC5E58DADEB5A632AB92F29772BF2B2D6BA", cert.Thumbprint);
        }
#endif

    }
}
