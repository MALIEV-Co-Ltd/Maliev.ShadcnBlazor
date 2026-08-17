using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;

namespace Maliev.ShadcnBlazor.BrowserTests;

public sealed class ComponentCatalogProofTests
{
    private static readonly string[] Completed = ["alert", "data-table", "date-picker"];

    [Fact]
    public void RequestedSlugsAreTrimmedAndReturnedInCatalogOrder()
    {
        var selected = ComponentCatalogProof.SelectRequested(Completed, " date-picker;alert ");

        Assert.Equal(["alert", "date-picker"], selected);
    }

    [Fact]
    public void EmptySelectionKeepsTheCompleteCatalog()
    {
        Assert.Same(Completed, ComponentCatalogProof.SelectRequested(Completed, null));
    }

    [Fact]
    public void UnknownRequestedSlugFailsClosed()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            ComponentCatalogProof.SelectRequested(Completed, "alert,unknown"));

        Assert.Contains("unknown", error.Message, StringComparison.Ordinal);
    }
}
