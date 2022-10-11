using MQTTnet.Client;

namespace MQTTnet.Extensions.MultiCloud.UnitTests.HubClient
{
    internal class AComplexObj
    {
        public string MyProperty { get; set; } = "initial value";
    }

    public class TwinWritablePropertyFixture
    {
        //private static string Stringify(object o) => System.Text.Json.JsonSerializer.Serialize(o);

        //private readonly IMqttClient connection;

        public TwinWritablePropertyFixture()
        {
           // connection = new MockMqttClient();

        }

        //[Fact]
        //public async Task InitEmptyTwin()
        //{
        //    WritableProperty<double> wp = new WritableProperty<double>(connection, "blah");
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 } },
        //        desired = new Dictionary<string, object>() { { "$version", 1 } },
        //    });

        //    await wp.InitPropertyAsync(twin, 0.2);
        //    ((MockMqttClient)connection).SimulateNewMessage("$iothub/twin/res/204", string.Empty);
        //    Assert.Equal(0.2, wp.PropertyValue.Value);
        //    Assert.Equal(0, wp.PropertyValue.Version);
        //    Assert.Equal(203, wp.PropertyValue.Status);
        //}

        //[Fact]
        //public async Task InitTwinWithReported()
        //{
        //    WritableProperty<double> wp = new WritableProperty<double>(connection, "myProp");
        //    string twin = Stringify(new
        //    {
        //        reported = new
        //        {
        //            myProp = new
        //            {
        //                ac = 203,
        //                av = 1,
        //                value = 4.3
        //            }
        //        },
        //        desired = new Dictionary<string, object>() { { "$version", 1 } },
        //    });

        //    await wp.InitPropertyAsync(twin, 0.2);
        //    Assert.Equal(4.3, wp.PropertyValue.Value);
        //    Assert.Equal(1, wp.PropertyValue.Version);
        //    Assert.Equal(203, wp.PropertyValue.Status);
        //}

        //[Fact]
        //public async Task InitTwinWithReportedComplex()
        //{
        //    var wpComplexObj = new WritableProperty<AComplexObj>(connection, "myComplexObj");
        //    string twin = Stringify(new
        //    {
        //        reported = new
        //        {
        //            myComplexObj = new
        //            {
        //                ac = 203,
        //                av = 1,
        //                value = new
        //                {
        //                    MyProperty = "fake twin value"
        //                }
        //            }
        //        },
        //        desired = new Dictionary<string, object>() { { "$version", 1 } },
        //    });

        //    await wpComplexObj.InitPropertyAsync(twin, new AComplexObj());
        //    Assert.Equal("fake twin value", wpComplexObj.PropertyValue.Value.MyProperty);
        //    Assert.Equal(1, wpComplexObj.PropertyValue.Version);
        //    Assert.Equal(203, wpComplexObj.PropertyValue.Status);
        //}

        //[Fact]
        //public async Task InitTwinWithDesiredTriggersUpdate()
        //{
        //    WritableProperty<double> wp = new WritableProperty<double>(connection, "myDouble");
        //    Assert.Equal(0, wp.PropertyValue.Value);
        //    bool received = false;
        //    wp.OnProperty_Updated = p =>
        //    {
        //        received = true;
        //        p.Status = 200;
        //        return p;
        //    };
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 } },
        //        desired = new Dictionary<string, object>() { { "$version", 2 }, { "myDouble", 2.3 } }
        //    });
        //    await wp.InitPropertyAsync(twin, 1);
        //    Assert.True(received);
        //    Assert.Equal(2.3, wp.PropertyValue.Value);
        //}

        //[Fact]
        //public async Task InitInvalidTwinWithDesiredTriggersUpdate()
        //{
        //    WritableProperty<double> wp = new WritableProperty<double>(connection, "myDouble");
        //    Assert.Equal(0, wp.PropertyValue.Value);
        //    bool received = false;
        //    wp.OnProperty_Updated = p =>
        //    {
        //        received = true;
        //        if (p.Value < 0)
        //        {
        //            p.Status = 405;
        //            p.Description = "fakie not accepted";
        //            p.Value = p.LastReported > 0 ?
        //                           p.LastReported :
        //                           1;
        //        }
        //        return p;
        //    };
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 } },
        //        desired = new Dictionary<string, object>() { { "$version", 2 }, { "myDouble", -1 } }
        //    });
        //    await wp.InitPropertyAsync(twin, 1);
        //    Assert.True(received);
        //    Assert.Equal(405, wp.PropertyValue.Status);
        //    Assert.Equal("fakie not accepted", wp.PropertyValue.Description);
        //    Assert.Equal(1, wp.PropertyValue.Value);
        //}

        //[Fact]
        //public async Task InitInvalidTwinWithDesiredAndReportedTriggersUpdate()
        //{
        //    WritableProperty<double> wp = new WritableProperty<double>(connection, "myDouble");
        //    Assert.Equal(0, wp.PropertyValue.Value);
        //    bool received = false;
        //    wp.OnProperty_Updated = p =>
        //    {
        //        received = true;
        //        if (p.Value < 0)
        //        {
        //            p.Status = 405;
        //            p.Description = "fakie not accepted";
        //            p.Value = p.LastReported;

        //        }
        //        return p;
        //    };
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 }, { "myDouble", new { ac = 203, value = 43 } } },
        //        desired = new Dictionary<string, object>() { { "$version", 2 }, { "myDouble", -1 } }
        //    });
        //    await wp.InitPropertyAsync(twin, 1);
        //    Assert.True(received);
        //    Assert.Equal(405, wp.PropertyValue.Status);
        //    Assert.Equal("fakie not accepted", wp.PropertyValue.Description);
        //    Assert.Equal(43, wp.PropertyValue.Value);
        //}


        //[Fact]
        //public async Task InitTwinComplexWithDesiredTriggersUpdate()
        //{
        //    WritableProperty<AComplexObj> wp = new WritableProperty<AComplexObj>(connection, "myComplexObj");
        //    Assert.Null(wp.PropertyValue.Value);
        //    wp.OnProperty_Updated = p =>
        //    {
        //        p.Status = 200;
        //        return p;
        //    };
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 } },
        //        desired = new Dictionary<string, object>() { { "$version", 2 }, { "myComplexObj", new AComplexObj { MyProperty = "twinValue" } } }
        //    });
        //    await wp.InitPropertyAsync(twin, new AComplexObj());
        //    Assert.Equal("twinValue", wp?.PropertyValue?.Value?.MyProperty);
        //    Assert.Equal(200, wp?.PropertyValue.Status);
        //    Assert.Equal(2, wp?.PropertyValue.Version);
        //    Assert.Equal(2, wp?.PropertyValue.DesiredVersion);
        //}


        //[Fact]
        //public async Task InitComponentTwinWithDesiredComponent()
        //{
        //    var wpWithComp = new WritableProperty<double>(connection, "myProp", "myComp");
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 } },
        //        desired = new Dictionary<string, object>() {
        //            {
        //                "$version", 2
        //            },
        //            {

        //                "myComp", new Dictionary<string, object>() {
        //                    {
        //                        "__t", "c"
        //                    },
        //                    {
        //                        "myProp", 3.4
        //                    }
        //                }
        //            }
        //        }
        //    });
        //    await wpWithComp.InitPropertyAsync(twin, 0.2);
        //    Assert.Equal(3.4, wpWithComp.PropertyValue.Value);
        //    Assert.Null(wpWithComp.PropertyValue.Description);
        //    Assert.Equal(0, wpWithComp.PropertyValue.Status);
        //    Assert.Equal(2, wpWithComp.PropertyValue.DesiredVersion);
        //}

        //[Fact]
        //public async Task InitComplexComponentTwinWithDesiredComponent()
        //{
        //    var wpWithComp = new WritableProperty<AComplexObj>(connection, "myComplexObj", "myComp");
        //    string twin = Stringify(new
        //    {
        //        reported = new Dictionary<string, object>() { { "$version", 1 } },
        //        desired = new Dictionary<string, object>() {
        //            {
        //                "$version", 2
        //            },
        //            {

        //                "myComp", new Dictionary<string, object>() {
        //                    {
        //                        "__t", "c"
        //                    },
        //                    {
        //                        "myComplexObj", new AComplexObj { MyProperty = "twinValue"}
        //                    }
        //                }
        //            }
        //        }
        //    }); ;
        //    await wpWithComp.InitPropertyAsync(twin, new AComplexObj());
        //    Assert.Equal("twinValue", wpWithComp.PropertyValue.Value.MyProperty);
        //    Assert.Null(wpWithComp.PropertyValue.Description);
        //    Assert.Equal(0, wpWithComp.PropertyValue.Status);
        //    Assert.Equal(2, wpWithComp.PropertyValue.DesiredVersion);
        //    Assert.Equal(2, wpWithComp.PropertyValue.Version);
        //}


        //[Fact]
        //public async Task InitTwinWithReportedInComponent()
        //{
        //    var wpWithComp = new WritableProperty<double>(connection, "myProp", "myComp");
        //    string twin = Stringify(new
        //    {
        //        desired = new Dictionary<string, object>()
        //        {
        //            { "$version", 3 }
        //        },
        //        reported = new Dictionary<string, object>()
        //        {
        //            {
        //                "$version", 4
        //            },
        //            {
        //                "myComp", new
        //                {
        //                    __t = "c",
        //                    myProp = new
        //                    {
        //                        ac = 200,
        //                        av = 3 ,
        //                        ad = "desc",
        //                        value = 3.4
        //                    }
        //                }
        //            }
        //        }
        //    });


        //    await wpWithComp.InitPropertyAsync(twin, 0.2);
        //    Assert.Equal(3.4, wpWithComp.PropertyValue.Value);
        //    Assert.Equal("desc", wpWithComp.PropertyValue.Description);
        //    Assert.Equal(200, wpWithComp.PropertyValue.Status);
        //    Assert.Equal(0, wpWithComp.PropertyValue.DesiredVersion);
        //}

        //[Fact]
        //public async Task InitTwinWithReportedInComponentWithoutFlag()
        //{
        //    var wpWithComp = new WritableProperty<double>(connection, "myProp", "myComp");
        //    string twin = Stringify(new
        //    {
        //        desired = new Dictionary<string, object>()
        //        {
        //            { "$version", 3 }
        //        },
        //        reported = new Dictionary<string, object>()
        //        {
        //            {
        //                "$version", 4
        //            },
        //            {
        //                "myComp", new
        //                {
        //                    myProp = new
        //                    {
        //                        ac = 200,
        //                        av = 3 ,
        //                        ad = "desc",
        //                        value = 3.4
        //                    }
        //                }
        //            }
        //        }
        //    });

        //    await wpWithComp.InitPropertyAsync(twin, 0.2);
        //    Assert.Equal(0.2, wpWithComp.PropertyValue.Value);
        //    Assert.Equal("Init from default value", wpWithComp.PropertyValue.Description);
        //    Assert.Equal(203, wpWithComp.PropertyValue.Status);
        //    Assert.Null(wpWithComp.PropertyValue.DesiredVersion);
        //}

        //[Fact]
        //public async Task InitReportedDoubleInComponent()
        //{
        //    var wpWithComp = new WritableProperty<double>(connection, "myProp", "myComp");
        //    string twin = Stringify(new
        //    {
        //        reported = new
        //        {
        //            myComp = new
        //            {
        //                __t = "c",
        //                myProp = new
        //                {
        //                    ac = 203,
        //                    av = 1,
        //                    value = 4.3
        //                }
        //            }
        //        },
        //        desired = new Dictionary<string, object>() { { "$version", 1 } },
        //    });

        //    await wpWithComp.InitPropertyAsync(twin, 0.2);
        //    Assert.Equal(4.3, wpWithComp.PropertyValue.Value);
        //    Assert.Equal(1, wpWithComp.PropertyValue.Version);
        //    Assert.Equal(203, wpWithComp.PropertyValue.Status);
        //}

        //[Fact]
        //public async Task InitReportedDoubleInComponentWithouFlag()
        //{
        //    var wpWithComp = new WritableProperty<double>(connection, "myProp", "myComp");
        //    string twin = Stringify(new
        //    {
        //        reported = new
        //        {
        //            myComp = new
        //            {
        //                myProp = new
        //                {
        //                    ac = 203,
        //                    av = 1,
        //                    value = 4.3
        //                }
        //            }
        //        },
        //        desired = new Dictionary<string, object>() { { "$version", 1 } },
        //    });

        //    await wpWithComp.InitPropertyAsync(twin, 0.2);
        //    Assert.Equal(0.2, wpWithComp.PropertyValue.Value);
        //    Assert.Equal(0, wpWithComp.PropertyValue.Version);
        //    Assert.Equal(203, wpWithComp.PropertyValue.Status);
        //}

    }
}
