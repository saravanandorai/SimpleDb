# SimpleDb

TraceListener that can be used to write to SimpleDB. This can be used to trace message from System.Diagnostics to Amazon's Simple DB.

More details in https://www.nuget.org/packages/SimpleDB.TraceListener-Pre/1.0.0-beta

Example Configuration in Application Config file [app.config / web.config] is given below,

  <system.diagnostics>
    <trace autoflush="true">
      <listeners>
        <add name="simpledbListener" type="SimpleDB.TraceListener.SimpleDBTraceListener, SimpleDB.TraceListener"
                     SystemName="us-west-2"
                     AWSAccessKey="AKIAIVX7GOFTJ7A"
                     AWSSecretKey="Gwe61Bp1rcAFYN9Sv0rTuJKPM/SfHr+Gz"
                     IncludeTimeStamp="true"
                     TableName="myLogs"
                     IsEnabled="true"
                     initializeData="test"/>
      </listeners>
    </trace>
  </system.diagnostics>

This source code was created to be multi-tenant aware logger, hence I have added the TenantId attribute, however as the tenant context can be established in many ways, I have left it as empty. Any developer that wants to enable Multi-Tenant Logging features, can edit the code to point to thier exact source of tenant identifier and use. 

Also, please share the comments / queries.
