using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class X509ChainValidatorFixture
    {
        [Fact]
        public void ValidateChainOneLevelOK()
        {
            X509Certificate cert = X509Certificate.CreateFromCertFile("onething.pem");
            var res = X509ChainValidator.ValidateChain(cert, "ca.pem");
            Assert.True(res);
        }

        //[Fact]
        //public void ValidateChainWithIntermediateOk()
        //{
        //    X509Certificate cert = X509Certificate.CreateFromCertFile("dev03.pem");
        //    var res = X509ChainValidator.ValidateChain(cert, "caWithChain.pem");
        //    Assert.True(res);
        //}

        //[Fact]
        //public void ValidateChainWithIntermediateBad()
        //{
        //    X509Certificate cert = X509Certificate.CreateFromCertFile("dev03.pem");
        //    var res = X509ChainValidator.ValidateChain(cert, "ca.pem");
        //    Assert.False(res);
        //}


    }
}
