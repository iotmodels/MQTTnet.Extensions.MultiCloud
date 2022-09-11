using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class BirthConventionFixture
    {
        [Fact]
        public void CanDeserialize()
        {
            var json = @"{""model-id"":""dtmi:rido:pnp:memmon;1"",""when"":""2022-09-11T16:28:38.1615346-07:00"",""status"":""offline""}";
            BirthConvention.BirthMessage bm = Json.FromString<BirthConvention.BirthMessage>(json)!;
            Assert.Equal(BirthConvention.ConnectionStatus.offline, bm.ConnectionStatus);
        }
    }
}
