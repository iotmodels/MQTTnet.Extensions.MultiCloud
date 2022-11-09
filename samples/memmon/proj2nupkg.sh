dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.AwsIoTClient/MQTTnet.Extensions.MultiCloud.AwsIoTClient.csproj 
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.AzureIoTClient/MQTTnet.Extensions.MultiCloud.AzureIoTClient.csproj
dotnet remove reference ../../src/MQTTnet.Extensions.MultiCloud.BrokerIoTClient/MQTTnet.Extensions.MultiCloud.BrokerIoTClient.csproj

dotnet add package MQTTnet.Extensions.MultiCloud.AzureIoTClient  --prerelease -n
dotnet add package MQTTnet.Extensions.MultiCloud.BrokerIoTClient --prerelease -n
dotnet add package MQTTnet.Extensions.MultiCloud.AwsIoTClient --prerelease -n

dotnet restore
dotnet list package