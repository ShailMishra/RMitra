namespace RM.DataModel.Common
{
    public class PingResponse
    {
        public string Application { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; }
        public bool DatabaseConnected { get; set; }
    }
}
