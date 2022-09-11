using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MQTTnet.Extensions.MultiCloud.Connections
{
    public static partial class MqttNetExtensions
    {
        public static MqttClientOptionsBuilder WithTlsSettings(this MqttClientOptionsBuilder builder, ConnectionSettings cs)
        {
            var tls = new MqttClientOptionsBuilderTlsParameters();
            tls.UseTls = cs.UseTls;
            if (cs.UseTls)
            {
                var certs = new List<X509Certificate2>();
                if (!string.IsNullOrEmpty(cs.X509Key))
                {
                    var cert = X509ClientCertificateLocator.Load(cs.X509Key);
                    certs.Add(cert);
                }
                
                if (!string.IsNullOrEmpty(cs.CaFile))
                {
                    var caCert = new X509Certificate2(cs.CaFile);
                    certs.Add(caCert);
                    tls.CertificateValidationHandler = ea =>
                    {
#if NET6_0
                        X509Chain chain = new X509Chain();
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                        chain.ChainPolicy.VerificationTime = DateTime.Now;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
                        chain.ChainPolicy.CustomTrustStore.Add(caCert);
                        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                        var x5092 = new X509Certificate2(ea.Certificate);
                        var res = chain.Build(x5092);
                        return res;
#endif
#if NETSTANDARD2_1
                    return ea.Certificate.Issuer == caCert.Subject;
#endif
                    };
                }
                tls.Certificates = certs;
                tls.IgnoreCertificateRevocationErrors = cs.DisableCrl;
                builder.WithTls(tls);
            }
            return builder;
        }
    }
}
