namespace SimpleDB.TraceListener
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Amazon.SimpleDB;
    using Amazon.SimpleDB.Model;
    using Newtonsoft.Json;
    using System.Globalization;
    using static Config.Constants;
    using static Config.DBColumnNames;
    using Amazon.Runtime;
    using Config;

    public class SimpleDBTraceListener : TraceListener
    {
        private static AmazonSimpleDBClient _client;

        public SimpleDBTraceListener() : base()
        {
        }
        public SimpleDBTraceListener(string name) : base(name)
        {
        }

        private void Init()
        {
            if (Configuration.StoredProfileCredentials == null)
            {
                _client = new AmazonSimpleDBClient(Configuration.AccessKeyId, Configuration.AccessSecretKey, Configuration.RegionEndPoint);
            }
            else
            {
                _client = new AmazonSimpleDBClient(Configuration.StoredProfileCredentials, Configuration.RegionEndPoint);
            }

            InitDomain(_client);
        }

        private void InitDomain(AmazonSimpleDBClient client)
        {
            var domains = client.ListDomains();

            var found = domains.DomainNames.Any(d => d.Equals(Configuration.TableName));

            if (!found)
            {
                client.CreateDomain(new CreateDomainRequest(Configuration.TableName));
            }
        }

        private static TraceEventCache traceEventCache = new TraceEventCache();

        public override void Write(string message)
        {
            TraceData(traceEventCache, this.Name, TraceEventType.Information, 0, message);
        }

        public override void WriteLine(string message)
        {
            TraceData(traceEventCache, this.Name, TraceEventType.Information, 0, message);
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] {
                CONFIG_ACCESSKEY , CONFIG_SECRETKEY , CONFIG_ISTIMESTAMPINCLUDED , CONFIG_TABLENAME , CONFIG_SYSTEMNAME , CONFIG_PROFILELOCATION , CONFIG_PROFILENAME , CONFIG_AUTHENTICATIONREGION , CONFIG_AUTHENTICATIONSERVICENAME , CONFIG_MAXERRORRETRY , CONFIG_MAXIDLETIME , CONFIG_SERVICEURL,CONFIG_ISENABLED
            };
        }

        public override bool IsThreadSafe
        {
            get
            {
                return false;
            }
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            Log(eventCache, source, eventType, id, string.Empty, data);
        }

        private void Log(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, object data)
        {
            if (!Configuration.IsEnabled) return;

            Init();

            var proc = Process.GetCurrentProcess();
            string machineName = proc.MachineName;

            if (machineName == ".")
            {
                machineName = Environment.MachineName;
            }

            if (data != null && string.IsNullOrEmpty(message))
                message += JsonConvert.SerializeObject(data);

            var attributes = new List<ReplaceableAttribute>
            {
                new ReplaceableAttribute(MachineName, machineName, true),
                new ReplaceableAttribute(Message, message, true),
                new ReplaceableAttribute(Source, source, true),
                new ReplaceableAttribute(Time , DateTime.Now.ToString("F"), true),
                new ReplaceableAttribute(ProcessId, proc.Id.ToString(), true),
                new ReplaceableAttribute(ProcessName, proc.ProcessName, true),
                new ReplaceableAttribute(ThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId.ToString(), true),
                new ReplaceableAttribute(EventId, id.ToString(), true),
                new ReplaceableAttribute(EventType, eventType.ToString("F"), true),
                new ReplaceableAttribute(UserId, System.Threading.Thread.CurrentPrincipal?.Identity?.Name, true),
                new ReplaceableAttribute(TenantId, "", true) //TODO: The developers can pass this value
            };

            if (eventCache != null && !string.IsNullOrEmpty(eventCache.Callstack) && eventCache.Callstack.Length < 1024)
                attributes.Add(new ReplaceableAttribute(CallStack, eventCache?.Callstack, true));

            _client.PutAttributes(new PutAttributesRequest
            {
                Attributes = attributes,
                DomainName = Configuration.TableName,
                ItemName = Guid.NewGuid().ToString()
            });
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            Log(eventCache, source, eventType, id, string.Empty, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            Log(eventCache, source, eventType, id, string.Empty, null);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            Log(eventCache, source, eventType, id, string.Format(CultureInfo.InvariantCulture, format, args), null);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            Log(eventCache, source, eventType, id, message, null);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            Log(eventCache, source, TraceEventType.Transfer, id, message, relatedActivityId);
        }

        public override void Write(object o)
        {
            Log(traceEventCache, null, TraceEventType.Information, 0, null, o);
        }

        public override void Write(object o, string category)
        {
            Log(traceEventCache, category, TraceEventType.Information, 0, null, null);
        }

        public override void Write(string message, string category)
        {
            Log(traceEventCache, category, TraceEventType.Information, 0, message, null);
        }

        public override void WriteLine(object o)
        {
            Log(traceEventCache, null, TraceEventType.Information, 0, string.Empty, o);
        }

        public override void WriteLine(object o, string category)
        {
            Log(traceEventCache, category, TraceEventType.Information, 0, string.Empty, o);
        }

        public override void WriteLine(string message, string category)
        {
            Log(traceEventCache, category, TraceEventType.Information, 0, message, null);
        }

        public override void Flush()
        {
        }

        protected override void WriteIndent()
        {
        }

        public override void Close()
        {
            base.Close();
        }
        #region configurations
        private static AmazonSimpleDBConfiguration _config;
        public AmazonSimpleDBConfiguration Configuration
        {
            get
            {
                if (_config != null)
                    return _config;

                _config = GetConfig();
                return _config;
            }
        }
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public bool IncludeTimeStamp { get; set; }
        public string TableName { get; set; }
        public string SystemName { get; set; }
        public string AWSProfileLocation { get; set; }
        public string AWSProfileName { get; set; }
        public string AuthenticationRegion { get; set; }
        public string AuthenticationServiceName { get; set; }
        public int MaxErrorRetry { get; set; }
        public int MaxIdleTime { get; set; }
        public string ServiceURL { get; set; }
        public bool IsEnabled { get; set; }

        public AmazonSimpleDBConfiguration GetConfig()
        {
            var config = new AmazonSimpleDBConfiguration
            {
                IncludeTimeStamp = GetAttributeValueAsBoolean(CONFIG_ISTIMESTAMPINCLUDED),
                TableName = GetAttributeValueAsString(CONFIG_TABLENAME),
                RegionEndPoint = GetEndpointBySystemName(CONFIG_SYSTEMNAME),
                StoredProfileCredentials = GetStoredProfileAWSCredential(),
                AccessKeyId = GetAttributeValueAsString(CONFIG_ACCESSKEY),
                AccessSecretKey = GetAttributeValueAsString(CONFIG_SECRETKEY),
                AuthenticationRegion = GetAttributeValueAsString(CONFIG_AUTHENTICATIONREGION),
                AuthenticationServiceName = GetAttributeValueAsString(CONFIG_AUTHENTICATIONSERVICENAME),
                MaxErrorRetry = GetAttributeValueAsInt(CONFIG_MAXERRORRETRY),
                MaxIdleTime = GetAttributeValueAsInt(CONFIG_MAXIDLETIME),
                ServiceURL = GetAttributeValueAsString(CONFIG_SERVICEURL),
                IsEnabled = GetAttributeValueAsBoolean(CONFIG_ISENABLED)
            };
            return config;
        }
        public StoredProfileAWSCredentials GetStoredProfileAWSCredential()
        {
            var profileName = GetAttributeValueAsString(CONFIG_PROFILENAME);

            if (!string.IsNullOrEmpty(profileName))
            {
                var profileLocation = GetAttributeValueAsString(CONFIG_PROFILELOCATION);

                return string.IsNullOrEmpty(profileLocation) ? new StoredProfileAWSCredentials(profileName) : new StoredProfileAWSCredentials(profileName, profileLocation);
            }
            else
            {
                return null;
            }
        }

        private Amazon.RegionEndpoint GetEndpointBySystemName(string configName)
        {
            var systemName = GetAttributeValueAsString(configName);

            if (!string.IsNullOrEmpty(systemName))
            {
                return Amazon.RegionEndpoint.GetBySystemName(systemName);
            }
            return null;
        }

        private int GetAttributeValueAsInt(string name)
        {
            int result = 0;
            if (int.TryParse(GetAttributeValueAsString(name, "0"), out result))
                return result;

            return result;
        }


        private bool GetAttributeValueAsBoolean(string name)
        {
            bool result = false;
            if (bool.TryParse(GetAttributeValueAsString(name, "false"), out result))
                return result;

            return result;
        }

        private string GetAttributeValueAsString(string name, string defaultValue = "")
        {
            if (Attributes.ContainsKey(name))
                return Attributes[name];
            else
                return defaultValue;
        }

        #endregion
    }
}
