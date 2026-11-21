using MowTrackr.Maintenance.Models;

namespace MowTrackr.Maintenance.Usages;

/// <summary>
/// Reports service templates with a step that uses a unit, so the unit cannot be deleted while they do.
/// </summary>
/// <param name="templates">The service template repository.</param>
internal sealed class ServiceTemplateUnitUsage(IRepository<ServiceTemplate> templates) : IUnitOfMeasureUsage
{
    /// <inheritdoc />
    public async Task<string?> DescribeUseAsync(Guid unitId, CancellationToken cancellationToken)
    {
        var items = await templates
            .ListAsync(new QuerySpec<ServiceTemplate> { Filter = t => t.Tasks.Any(k => k.UnitId == unitId) }, cancellationToken)
            .ConfigureAwait(false);

        return items.Count == 0 ? null : $"{items.Count} service template(s)";
    }
}
