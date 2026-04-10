using Xunit;

namespace Legacy89DiskKit.Tests;

public class HostProofExpectationCatalogTest
{
    [Fact]
    public void Catalog_CanReturnEventDrivenD88Baseline()
    {
        var expectation = HostProofExpectationCatalog.EventDrivenFirstProofD88();

        Assert.True(expectation.RequirePathOpen);
        Assert.True(expectation.RequireNotificationExchange);
        Assert.True(expectation.RequireClose);
    }

    [Fact]
    public void Catalog_CanReturnEventDrivenRawBaseline()
    {
        var expectation = HostProofExpectationCatalog.EventDrivenSecondProofRaw();

        Assert.True(expectation.RequireBufferOpen);
        Assert.False(expectation.RequirePathOpen);
        Assert.False(expectation.RequireClose);
    }
}
