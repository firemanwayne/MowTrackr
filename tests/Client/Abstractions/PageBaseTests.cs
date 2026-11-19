using MowTrackr.Client.Abstractions;

namespace MowTrackr.Client.UnitTests.Abstractions;

public sealed class PageBaseTests
{
    [Fact]
    public async Task RunOnceAsync_PageLeftWhileTheActionRuns_DoesNotThrowWhenTheCooldownEnds()
    {
        // A successful sign-in navigates away (disposing the page) inside the action; the guard's cooldown then
        // finishes on a disposed page. That used to raise ObjectDisposedException, shown to users as the yellow
        // "An unhandled error has occurred" bar.
        var page = new TestPage();

        await page.RunAsync("signin", () =>
        {
            page.Dispose();

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task RunOnceAsync_SameKeyWhileRunning_IsIgnored()
    {
        var page = new TestPage();

        var runs = 0;

        var release = new TaskCompletionSource();

        var first = page.RunAsync("save", async () =>
        {
            runs++;

            await release.Task;
        });

        await page.RunAsync("save", () =>
        {
            runs++;

            return Task.CompletedTask;
        });

        Assert.True(page.Running("save"));

        // Leave the page before the cooldown ends so the test does not need a renderer.
        page.Dispose();

        release.SetResult();

        await first;

        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task RunOnceAsync_DifferentKeys_DoNotBlockEachOther()
    {
        var page = new TestPage();

        var runs = 0;

        var release = new TaskCompletionSource();

        var first = page.RunAsync("save", async () =>
        {
            runs++;

            await release.Task;
        });

        var second = page.RunAsync("delete", () =>
        {
            runs++;

            return Task.CompletedTask;
        });

        Assert.Equal(2, runs);

        // Leave the page before the cooldowns end so the test does not need a renderer.
        page.Dispose();

        release.SetResult();

        await Task.WhenAll(first, second);
    }
    [Fact]
    public async Task RunOnceAsync_ActionThrows_StillReleasesTheKey()
    {
        var page = new TestPage();

        await Assert.ThrowsAsync<InvalidOperationException>(() => page.RunAsync("save", () => throw new InvalidOperationException("boom")));

        // Leaving the page ends the cooldown at once, so the key is free again.
        Assert.False(page.Running("save"));
    }

    private sealed class TestPage : PageBase
    {
        public Task RunAsync(string key, Func<Task> action)
        {
            return RunOnceAsync(key, action);
        }

        public bool Running(string key)
        {
            return IsRunning(key);
        }
    }
}