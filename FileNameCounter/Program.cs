using System.IO.Abstractions;
using FileNameCounter.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FileNameCounter.Interfaces;

var builder = ProgramHelper.SuitableForBothApplicationAndTest();
builder.Services.AddSingleton<IFileSystem, FileSystem>();

using IHost host = builder.Build();
using IServiceScope serviceScope = host.Services.CreateScope();
var main = serviceScope.ServiceProvider.GetRequiredService<IMain>();
var output = await main.Run(args);
Console.WriteLine(output);

//host is not run as we are only interested in DI

public static class ProgramHelper
{
    public static HostApplicationBuilder SuitableForBothApplicationAndTest()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IArgumentProcessor, ArgumentProcessor>();
        builder.Services.AddSingleton<IStringInstanceCounterFactory, StringInstanceCounterSpanBasedSimplifiedFactory>(
            p => new StringInstanceCounterSpanBasedSimplifiedFactory(1024));
        builder.Services.AddSingleton<IMain, Main>();
        return builder;
    }
}