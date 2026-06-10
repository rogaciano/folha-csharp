using FluentAssertions;
using RhFolha.Domain.Production;

namespace RhFolha.Tests.Production;

public sealed class ProductionRateTableTests
{
    [Fact]
    public void New_table_starts_as_draft()
    {
        var table = new ProductionRateTable(Guid.NewGuid(), "Tabela producao", new DateOnly(2026, 6, 1));

        table.Status.Should().Be("Draft");
        table.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Can_activate_and_deactivate_table_without_losing_history()
    {
        var table = new ProductionRateTable(Guid.NewGuid(), "Tabela producao", new DateOnly(2026, 6, 1));

        table.Activate();
        table.Deactivate();

        table.Status.Should().Be("Inactive");
        table.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Deleting_rate_marks_virtual_deletion()
    {
        var rate = new ProductionRate(Guid.NewGuid(), Guid.NewGuid(), 1.25m);

        rate.Delete();

        rate.DeletedAt.Should().NotBeNull();
    }
}
