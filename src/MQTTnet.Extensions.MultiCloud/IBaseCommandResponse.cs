namespace MQTTnet.Extensions.MultiCloud
{
    public interface IBaseCommandResponse
    {
        public int Status { get; set; }
        public object ReponsePayload { get; set; }
    }
}
