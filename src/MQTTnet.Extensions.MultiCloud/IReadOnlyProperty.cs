using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface IReadOnlyProperty<T>
    {
        string PropertyName { get; }
        T PropertyValue { get; set; }
        int Version { get; set; }
        Task<int> ReportPropertyAsync(CancellationToken cancellationToken = default);
        Task<int> ReportPropertyAsync(byte[] payload, CancellationToken cancellationToken = default);
    }
}