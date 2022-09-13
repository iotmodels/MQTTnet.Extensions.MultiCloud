dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.AwsIoTClient/MQTTnet.Extensions.MultiCloud.AwsIoTClient.csproj 
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.AzureIoTClient/MQTTnet.Extensions.MultiCloud.AzureIoTClient.csproj
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.BrokerIoTClient/MQTTnet.Extensions.MultiCloud.BrokerIoTClient.csproj

dotnet add package MQTTnet.Extensions.MultiCloud.AzureIoTClient 
dotnet add package MQTTnet.Extensions.MultiCloud.BrokerIoTClient 
dotnet add package MQTTnet.Extensions.MultiCloud.AwsIoTClient 