using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

namespace MQTTnet.Extensions.MultiCloud.Clients.Connections
{
    public class X509ClientCertificateLocator
    {
        // TODO: support .PEM
        public static X509Certificate2 Load(string certSettings)
        {
            X509Certificate2? cert = null;
            if (certSettings.Contains(".pfx|")) // mycert.pfx|mypwd
            {
                var segments = certSettings.Split('|');
                string path = segments[0];
                var pwd = segments[1];
                cert = new X509Certificate2(path, pwd);
            }
            else if (certSettings.Length == 40) //thumbprint
            {
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, certSettings, false);
                    if (certs != null && certs.Count > 0)
                    {
                        cert = certs[0];
                    }
                    store.Close();
                }
            }
            else if (certSettings.Contains(".pem|")) //mycert.pem|mycert.key
            {
#if NET6_0_OR_GREATER
                var segments = certSettings.Split('|');
                var pemPath = segments[0];
                var keyPath = segments[1];

                if (segments.Length == 2)
                {
                    var thisCert = X509Certificate2.CreateFromPemFile(pemPath, keyPath);
                    // https://github.com/dotnet/runtime/issues/45680#issuecomment-739912495
                    cert = new X509Certificate2(thisCert.Export(X509ContentType.Pkcs12));
                }
                if (segments.Length == 3)
                {
                    var keyPasswd = segments[2];
                    var thisCert = X509Certificate2.CreateFromEncryptedPemFile(pemPath, keyPasswd, keyPath);
                    cert = new X509Certificate2(thisCert.Export(X509ContentType.Pkcs12));
                }
#else
                throw new NotSupportedException("PEM files not supported before net6");
#endif
            }
            else
            {
                throw new KeyNotFoundException("certSettings format not recognized");
            }

            if (cert == null)
            {
                throw new KeyNotFoundException("cert not found");
            }

            if (!cert.HasPrivateKey)
            {
                Trace.TraceWarning("Cert found with no private key");
            }
            Trace.TraceInformation("Loaded Cert: " + cert.SubjectName.Name + " " + cert.Thumbprint);
            return cert;
        }
    }
}
