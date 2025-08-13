using Serilog.Sinks.MSSqlServer;

using System.Data;

namespace LoggingModule.Configurations;

public class SqlColumns
{
    public static ColumnOptions ColumnOptions()
    {
        var options = new ColumnOptions();
        options.Store.Remove(StandardColumn.Properties);
        options.Store.Remove(StandardColumn.MessageTemplate);
        options.Store.Remove(StandardColumn.Exception);

        options.AdditionalColumns =
        [
            new SqlColumn("Elapsed", SqlDbType.Float) { AllowNull = true },
            new SqlColumn("Method", SqlDbType.NVarChar, dataLength: 10) { AllowNull = true },
            new SqlColumn("Path", SqlDbType.NVarChar, dataLength: 2048) { AllowNull = true },
            new SqlColumn("QueryString", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("RequestHeaders", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("RequestBody", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("ResponseStatusCode", SqlDbType.Int) { AllowNull = true },
            new SqlColumn("ResponseHeaders", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("ResponseBody", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("ExceptionDetails", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("FilterType", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("Hostname", SqlDbType.NVarChar, dataLength: 512) { AllowNull = true },
            new SqlColumn("ControllerAction", SqlDbType.NVarChar, dataLength: -1) { AllowNull = true },
            new SqlColumn("IpAddress", SqlDbType.NVarChar, dataLength: 100) { AllowNull = true },
            new SqlColumn("MemoryUsage", SqlDbType.Float) { AllowNull = true },
            new SqlColumn("UserAgent", SqlDbType.NVarChar, dataLength: 450) { AllowNull = true },
        ];
        return options;
    }
}