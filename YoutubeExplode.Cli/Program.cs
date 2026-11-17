using System.Threading.Tasks;
using CliFx;

namespace YoutubeExplode.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args) =>
        await new CommandLineApplicationBuilder()
            .SetDescription("Command-line interface for YoutubeExplode")
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync(args);
}
