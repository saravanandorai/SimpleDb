using Amazon.Runtime;
using Amazon.SimpleDB;

namespace SimpleDB.TraceListener.Config
{
    public class AmazonSimpleDBConfiguration : AmazonSimpleDBConfig
    {
        public bool IncludeTimeStamp { get; set; }
        public string TableName { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessSecretKey { get; set; }
        public StoredProfileAWSCredentials StoredProfileCredentials { get; set; }
        public Amazon.RegionEndpoint RegionEndPoint { get; set; }
        public bool IsEnabled { get; set; }
    }

    public static class DBColumnNames
    {
        public static string MachineName = "MachineName", Message = "Message", Time = "TimeStamp", CallStack = "CallStack", ProcessId = "ProcessId", ProcessName = "ProcessName", ThreadId = "ThreadId", EventId = "EventId", EventType = "EventType", UserId = "UserId", TenantId = "TenantId", Source = "Source";
    }
}