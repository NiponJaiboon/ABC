using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace API.Extensions
{
    public static class LoggingExtensions
    {
        public static void ConfigureSerilog(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information)
                    .WriteTo.File("Logs/Information/log-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Warning)
                    .WriteTo.File("Logs/Error/error-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"))
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}