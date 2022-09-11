using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface IPropertyStoreWriter
    {
        Task<int> ReportPropertyAsync(object payload, CancellationToken token = default);
    }
}
