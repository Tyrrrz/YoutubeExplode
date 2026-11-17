using System;
using System.Threading.Tasks;
using CliFx.Infrastructure;
using Gress;
using Spectre.Console;

namespace YoutubeExplode.Cli.Utils.Extensions;

internal static class ConsoleExtensions
{
    extension(IConsole console)
    {
        // Rich output goes to stderr, so that stdout remains reserved for data
        public IAnsiConsole CreateAnsiConsole() =>
            AnsiConsole.Create(
                new AnsiConsoleSettings
                {
                    Ansi = AnsiSupport.Detect,
                    ColorSystem = ColorSystemSupport.Detect,
                    Out = new AnsiConsoleOutput(console.Error),
                }
            );

        public Progress CreateProgressTicker() =>
            console
                .CreateAnsiConsole()
                .Progress()
                .AutoClear(false)
                .AutoRefresh(true)
                .HideCompleted(false)
                .Columns(
                    new TaskDescriptionColumn { Alignment = Justify.Left },
                    new ProgressBarColumn(),
                    new PercentageColumn()
                );

        public async ValueTask RunWithProgressAsync(
            string description,
            Func<IProgress<Percentage>, ValueTask> performOperationAsync
        )
        {
            await console
                .CreateProgressTicker()
                .StartAsync(async context =>
                {
                    var task = context.AddTask(
                        description,
                        new ProgressTaskSettings { MaxValue = 1 }
                    );

                    try
                    {
                        await performOperationAsync(
                            new DelegateProgress<Percentage>(p => task.Value = p.Fraction)
                        );
                    }
                    finally
                    {
                        task.Value = task.MaxValue;
                        task.StopTask();
                    }
                });
        }
    }
}
