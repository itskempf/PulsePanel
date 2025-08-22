using System;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using Xunit;

namespace PulsePanel.Tests;

public class ServerServiceTests
{
    [Fact]
    public async Task StartStop_UpdatesStatus_AndRaisesEvent()
    {
        // Arrange: real store on test base dir, ensure a server exists
        var logger = new ProvenanceLogger(System.IO.Path.Combine(AppContext.BaseDirectory, "provenance-tests.jsonl"));
        var process = new ServerProcessService(logger);
        var store = new ServerStore();
        var list = store.Load();
        var id = "test-srv-1";
        var existing = list.FirstOrDefault(s => s.Id == id);
        if (existing == null)
        {
            existing = new ServerEntry { Id = id, Name = "Test", Game = "dummy", Status = "stopped", InstallDir = "c:/tmp" };
            list.Add(existing);
            store.Save(list);
        }

        var svc = new ServerService(store, process);
        var raised = 0;
        svc.ServersChanged += (_, __) => raised++;

        // Act
        var okStart = await svc.StartAsync(id);
        var afterStart = await svc.GetServersAsync();
        var started = afterStart.First(s => s.Id == id);

        var okStop = await svc.StopAsync(id);
        var afterStop = await svc.GetServersAsync();
        var stopped = afterStop.First(s => s.Id == id);

        // Assert
        Assert.True(okStart);
        Assert.True(okStop);
        Assert.Equal("running", started.Status);
        Assert.Equal("stopped", stopped.Status);
        Assert.True(raised >= 2);
    }
}
