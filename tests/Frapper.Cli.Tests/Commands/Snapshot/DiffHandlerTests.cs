using Frapper.Cli.Commands.Diff;
using Xunit;

namespace Frapper.Cli.Tests.Commands.Diff;

public sealed class DiffHandlerTests
{
    [Fact]
    public async Task RunAsync_ShouldReturnOne_WhenBaseSnapshotDoesNotExist()
    {
        var handler = new DiffHandler();

        var exitCode = await handler.RunAsync(
            "missing-base.json",
            "missing-target.json",
            CancellationToken.None);

        Assert.Equal(1, exitCode);
    }
}