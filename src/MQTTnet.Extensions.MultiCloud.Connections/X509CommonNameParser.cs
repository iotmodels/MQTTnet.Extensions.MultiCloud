namespace MQTTnet.Extensions.MultiCloud.Connections
{
    public static class X509CommonNameParser
    {
        public static string GetCNFromCertSubject(string subject)
        {
            var result = subject.Substring(3);
            if (subject.Contains(','))
            {
                var posComma = result.IndexOf(',');
                result = result.Substring(0, result.Length - posComma);
            }
            return result.Replace(" ", "");
        }
    }
}
