
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.AwsIoTClient/MQTTnet.Extensions.MultiCloud.AwsIoTClient.csproj 
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.AzureIoTClient/MQTTnet.Extensions.MultiCloud.AzureIoTClient.csproj
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.BrokerIoTClient/MQTTnet.Extensions.MultiCloud.BrokerIoTClient.csproj

dotnet add package MQTTnet.Extensions.MultiCloud.AzureIoTClient -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/ridopackages/api/v3/index.json
dotnet add package MQTTnet.Extensions.MultiCloud.BrokerIoTClient -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/ridopackages/api/v3/index.json
dotnet add package MQTTnet.Extensions.MultiCloud.AwsIoTClient -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/ridopackages/api/v3/index.json