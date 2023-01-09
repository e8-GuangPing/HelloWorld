global using System;
global using System.Text;
global using System.Collections.Generic;



namespace ECP;
public static class Program
{
    public static void Main(string[] args)
    {
        // Task 1076487: Add global regex timeout to ECP to avoid risk of a ReDoS attack
        AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(5));

        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseKestrel()
            .UseStartup<Startup>()
            .ConfigureKestrel((context, options) =>
    {
        options.Limits.MaxConcurrentConnections = null;
		options.ConfigureHttpsDefaults(opt =>
        {
            opt.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
        });
    });
}