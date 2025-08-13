using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace LoggingModule.Configurations
{
    public static class LoggingServicesConfigurations
    {
        public static void AddLoggingServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddSerilog();
            builder.Host.UseSerilog();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            Log.Logger = new LoggerConfiguration()
                .Filter.ByIncludingOnly(e => e.Level is LogEventLevel.Error or LogEventLevel.Information)
                .Filter.ByIncludingOnly("FilterType = 'dummy_filter'")
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "Logs",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: SqlColumns.ColumnOptions()
                )
                .CreateLogger();

        }
    }
}
