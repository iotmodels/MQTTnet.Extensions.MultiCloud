using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.Clients
{
    public interface IWritableProperty<T>
    {
        string PropertyName { get; }
        PropertyAck<T> PropertyValue { get; set; }

        Func<PropertyAck<T>, Task<PropertyAck<T>>> OnProperty_Updated { get; set; }

        Task InitPropertyAsync(string twin, T defaultValue, CancellationToken cancellationToken = default);
        Task<int> ReportPropertyAsync(CancellationToken token = default);
    }
}