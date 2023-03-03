using MQTTnet.Extensions.MultiCloud.Connections;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    public class BirthConventionFixture
    {
        [Fact]
        public void CanDeserialize()
        {
            //var json = @"{""model-id"":""dtmi:rido:pnp:memmon;1"",""when"":""2022-09-11T16:28:38.1615346-07:00"",""status"":""offline""}";
            var json = @"{""model-id"":""dtmi:rido:pnp:sensehat;1"",""when"":""2023-02-08T08:39:40.9008917+00:00"",""status"":""online""}";
            BirthConvention.BirthMessage bm = Json.FromString<BirthConvention.BirthMessage>(json)!;
            Assert.Equal(BirthConvention.ConnectionStatus.offline, bm.ConnectionStatus);
        }
    }
}
