using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface IPropertyStoreReader
    {
        Task<string> ReadPropertiesDocAsync(CancellationToken cancellationToken = default);
    }
}
