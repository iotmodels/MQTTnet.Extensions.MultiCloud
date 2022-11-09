using MQTTnet.Client;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace MQTTnet.Extensions.MultiCloud.Connections
{
    internal static class X509ChainValidator
    {
        internal static bool ValidateChain(X509Certificate cert, string caCertFile)
        {
            X509Chain chain = new();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            chain.ChainPolicy.VerificationTime = DateTime.Now;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
            X509Certificate2 caCert = new X509Certificate2(caCertFile);
            chain.ChainPolicy.CustomTrustStore.Add(caCert);
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            var x5092 = new X509Certificate2(cert);
            var res = chain.Build(x5092);
            if (res == false)
            {
                Trace.TraceError($"Error validating TLS chain for cert: '{cert.Subject}' issued by '{cert.Issuer}', configured CA: '{caCert.Subject}'");
                chain.ChainStatus.ToList().ForEach(s => Trace.TraceError(s.StatusInformation));
            }
            return res;
        }
    }
}