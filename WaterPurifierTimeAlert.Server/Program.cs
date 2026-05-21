
using CommandLine;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Configuration;
using System.Net.Security;
using System.RelativeTime;
using System.RelativeTime.Parser;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace WaterPurifierTimeAlert.Server
{
    using Server.Context;
    using Server.Context.Store;

    public class Program
    {
        public sealed class CmdMain
        {
            [Option("config", Required = true, HelpText = "config file path")]
            public string ConfigFilePath { get; set; } = null!;

            [Option("log", Required = true, HelpText = "log dir path")]
            public string LogDirPath { get; set; } = null!;

            [Option("spaDevServerUrl", Default = "http://localhost:5172", HelpText = "ProxyToSpaDevelopmentServerUrl")]
            public string SpaDevServerUrl { get; set; } = null!;
        }

        public static async Task Main(string[] args)
        {
            ParserResult<CmdMain> result = await Parser.Default.ParseArguments<CmdMain>(args)
                .WithParsedAsync(async cmdMain =>
                {
                    YamlDotNet.Serialization.Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
                    FileInfo configFileInfo = new FileInfo(cmdMain.ConfigFilePath);
                    string str = File.ReadAllText(configFileInfo.FullName);
                    Configuration? configuration = deserializer.Deserialize<Configuration>(str);
                    WebApplication app = CreateWebApplication(args, cmdMain, configuration);
                    await StartAsync(app, cmdMain, configuration);
                });
        }

        private static WebApplication CreateWebApplication(string[] args, CmdMain cmdMain, Configuration configuration)
        {
            X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(
                new FileInfo(configuration.ServerCertificate).FullName,
                configuration.ServerCertificatePassword
            );

            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = configuration.ContentRootPath,
                WebRootPath = configuration.WebRootPath,
            });

            builder.Logging.Services.AddSerilog(configureLogger =>
            {
                configureLogger.Enrich.WithCaller()
                    .WriteTo.File(
                        new DirectoryInfo(cmdMain.LogDirPath).FullName,
                        "waterPurifierTimeAlert.log",
                        Serilog.Events.LogEventLevel.Information,
                        CallerEnricherOutputTemplate.Default,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 12
                    );
            });

            builder.WebHost.UseKestrel(options =>
            {
                if (configuration.WebHttpPort.HasValue)
                    options.ListenAnyIP(configuration.WebHttpPort.Value);
                if (configuration.WebHttpsPort.HasValue)
                {
                    options.ListenAnyIP(configuration.WebHttpsPort.Value, configure =>
                    {
                        configure.UseHttps(httpsOptions =>
                        {
                            httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                            httpsOptions.ServerCertificate = certificate;

                            if (configuration.CertificateChain is not null)
                            {
                                X509Certificate2Collection collection = X509CertificateLoader.LoadPkcs12CollectionFromFile(
                                    new FileInfo(configuration.ServerCertificate).FullName,
                                    configuration.ServerCertificatePassword
                                );

                                Dictionary<string, X509Certificate2> certificates = new Dictionary<string, X509Certificate2>();
                                foreach (X509Certificate2 certificate in collection.Where(cert => !cert.HasPrivateKey))
                                    certificates.Add(certificate.Thumbprint, certificate);

                                string[] certificateChain =
                                    configuration.CertificateChain.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                                httpsOptions.ServerCertificateChain = new X509Certificate2Collection();

                                foreach (string thumbprint in certificateChain)
                                    httpsOptions.ServerCertificateChain.Add(certificates[thumbprint]);
                            }

                            httpsOptions.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                            if (configuration.IncludeCipherSuites is not null)
                            {
                                string[] includeCipherSuites =
                                configuration.IncludeCipherSuites.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                                if (includeCipherSuites.Length > 0)
                                {
                                    httpsOptions.OnAuthenticate = (connectionContext, authenticationOptions) =>
                                    {
                                        authenticationOptions.AllowRenegotiation = false;
                                        HashSet<TlsCipherSuite> tlsCipherSuites = new HashSet<TlsCipherSuite>();
                                        foreach (string cipherSuite in includeCipherSuites)
                                        {
                                            if (Enum.TryParse(cipherSuite, out TlsCipherSuite tlsCipherSuite))
                                                tlsCipherSuites.Add(tlsCipherSuite);
                                        }


                                        if (tlsCipherSuites.Count > 0)
                                            authenticationOptions.CipherSuitesPolicy = new CipherSuitesPolicy(tlsCipherSuites);
                                    };
                                }
                            }
                        });
                    });
                }

                options.ConfigureEndpointDefaults(configureOptions =>
                {
                    configureOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            //// Add services to the container.

            builder.Services.AddSystemd();
            builder.Services.AddWindowsService();
            builder.Services.AddDbContextFactory<PurifierContext>(builder =>
            {
                DirectoryInfo? directory = new FileInfo(configuration.DbPath).Directory;
                if (directory is not null && !directory.Exists)
                    directory.Create();
                builder.UseSqlite($"Data Source={configuration.DbPath}");
                using PurifierContext context = new PurifierContext((DbContextOptions<PurifierContext>)builder.Options);
                context.Database.Migrate();
            });
            builder.Services.AddSingleton<IFilterTypeStore, FilterTypeStore>();
            builder.Services.AddSingleton<IExchangeFilterStore, ExchangeFilterStore>();
            builder.Services.AddSingleton(RelativeTimeExpressionParserFactory.Create());
            builder.Services.AddSingleton(RelativeTimeExpressionToDateTimeParserFactory.Create());
            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton<ITaskScheduler, DefaultTaskScheduler>();
            builder.Services.AddSignalR();
            builder.Services.AddControllers();
            //// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSpaStaticFiles(configure =>
            {
                configure.RootPath = configuration.WebRootPath;
            });
            builder.Services.AddHsts(configureOptions =>
            {
                configureOptions.Preload = true;
                configureOptions.IncludeSubDomains = true;
                configureOptions.MaxAge = TimeSpan.FromDays(365);
            });
            builder.Services.AddHttpsRedirection(configureOptions =>
            {
                if (configuration.WebHttpsPort.HasValue)
                    configureOptions.HttpsPort = configuration.WebHttpsPort.Value;
            });
            builder.Services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = "Certificate";
            }).AddCertificate("Certificate", configureOptions =>
            {
                configureOptions.AllowedCertificateTypes = CertificateTypes.All;
                configureOptions.RevocationMode = X509RevocationMode.NoCheck;
                configureOptions.Events = new CertificateAuthenticationEvents
                {
                    OnCertificateValidated = context =>
                    {
                        var cert = context.ClientCertificate;
                        if (cert is null)
                        {
                            context.Fail(new AuthenticationException());
                            return Task.CompletedTask;
                        }

                        context.Success();
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddAuthorization(configure =>
            {
                configure.AddPolicy("Certificate", policy =>
                {
                    policy.AddAuthenticationSchemes("Certificate");
                    policy.RequireAuthenticatedUser();
                });
            });


            return builder.Build();
        }

        private static async Task StartAsync(WebApplication app, CmdMain cmdMain, Configuration configuration)
        {
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseWhen(context => context.Connection.LocalPort == configuration.WebHttpPort, configure =>
            {
                configure.UseHttpsRedirection();
            });
            app.UseHsts();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapWhen(
                    context => !context.Request.Path.StartsWithSegments("/api"),
                    app => app.UseSpa(spa => spa.UseProxyToSpaDevelopmentServer(cmdMain.SpaDevServerUrl))
                );
            }
            else
            {
                app.MapFallbackToFile("index.html");
            }

            await app.RunAsync();
        }
    }
}
