using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.Clients
{
    public interface IPropertyStoreReader
    {
        Task<string> ReadPropertiesDocAsync(CancellationToken cancellationToken = default);
    }
}
