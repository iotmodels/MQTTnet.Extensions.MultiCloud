using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface IReportPropertyBinder
    {
        Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default);
    }
}