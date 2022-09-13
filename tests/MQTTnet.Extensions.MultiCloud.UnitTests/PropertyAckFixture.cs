using MQTTnet.Extensions.MultiCloud;
using System;
using Xunit;

namespace MQTTnet.Extensions.MultiCloud.UnitTests
{
    internal class AComplexObj
    {
        public string AStringProp { get; set; } = string.Empty;
        public int AIntProp { get; set; }
    }

    public class PropertyAckFixture
    {
        private static string Stringify(object o) => System.Text.Json.JsonSerializer.Serialize(o);

        [Fact]
        public void AckDouble()
        {
            var wp = new PropertyAck<double>("aDouble")
            {
                Description = "updated",
                Status = 200,
                Version = 3,
                Value = 1.2,
            };

            var expectedJson = Stringify(new
            {
                aDouble = new
                {
                    av = 3,
                    ad = "updated",
                    ac = 200,
                    value = 1.2,
                }
            });
            Assert.Equal(expectedJson, Stringify(wp.ToAckDict()));
        }

        [Fact]
        public void NullIgnored()
        {
            var wp = new PropertyAck<double>("aDouble")
            {
                Status = 200,
                Version = null,
                Value = 1.2,
            };

            var expectedJson = Stringify(new
            {
                aDouble = new
                {
                    ac = 200,
                    value = 1.2,
                }
            });
            Assert.Equal(expectedJson, Stringify(wp.ToAckDict()));
        }


        [Fact]
        public void AckDateTime()
        {
            var wpDate = new PropertyAck<DateTime>("aDateTime")
            {
                Value = new DateTime(2011, 11, 10, 8, 31, 12),
                Version = 3,
                Status = 200,
                Description = "updated"
            };

            var expectedJson = Stringify(new
            {
                aDateTime = new
                {
                    av = 3,
                    ad = "updated",
                    ac = 200,
                    value = "2011-11-10T08:31:12",
                }
            });
            Assert.Equal(expectedJson, Stringify(wpDate.ToAckDict()));
        }

        [Fact]
        public void AckBool()
        {
            var wpBoolean = new PropertyAck<bool>("aBoolean")
            {
                Value = false,
                Version = 3,
                Status = 200,
                Description = "updated"
            };

            var expectedJson = Stringify(new
            {
                aBoolean = new
                {
                    av = 3,
                    ad = "updated",
                    ac = 200,
                    value = false,
                }
            });
            Assert.Equal(expectedJson, Stringify(wpBoolean.ToAckDict()));
        }

        [Fact]
        public void AckComplexOject()
        {
            var aComplexObj = new AComplexObj() { AIntProp = 1, AStringProp = "a" };
            var prop = new PropertyAck<AComplexObj>("aComplexObj")
            {
                Version = 3,
                Value = aComplexObj,
                Status = 213,
                Description = "description"
            };
            var expectedJson = Stringify(new
            {
                aComplexObj = new
                {
                    av = 3,
                    ad = "description",
                    ac = 213,
                    value = new
                    {
                        AStringProp = "a",
                        AIntProp = 1
                    },
                }
            });
            Assert.Equal(expectedJson, Stringify(prop.ToAckDict()));
        }

        [Fact]
        public void AckDoubleInComponent()
        {
            var wp = new PropertyAck<double>("aDouble", "inAComp")
            {
                Version = 4,
                Status = 200,
                Value = 2.4,
                Description = "updated"
            };
            var expectedJson = Stringify(new
            {
                inAComp = new
                {
                    __t = "c",
                    aDouble = new
                    {
                        av = 4,
                        ad = "updated",
                        ac = 200,
                        value = 2.4,
                    }
                }
            });
            Assert.Equal(expectedJson, Stringify(wp.ToAckDict()));
        }

        [Fact]
        public void SetDefaultsInString()
        {
            var sp = new PropertyAck<string>("anString");
            Assert.Null(sp.Value);
            sp.SetDefault("def");
            Assert.Equal("def", sp.Value);
            Assert.Equal(0, sp.Version);
            Assert.Equal(203, sp.Status);

        }
    }
}
