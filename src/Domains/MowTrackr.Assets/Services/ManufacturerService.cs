using MowTrackr.Assets.Models;

namespace MowTrackr.Assets.Services;

/// <summary>
/// Default <see cref="IManufacturerService"/> backed by the generic repository.
/// </summary>
/// <param name="manufacturers">The manufacturer repository.</param>
/// <param name="models">The model repository.</param>
/// <param name="logger">The logger.</param>
internal sealed class ManufacturerService(
    IRepository<Manufacturer> manufacturers,
    IRepository<ManufacturerModel> models,
    ILogger<ManufacturerService> logger) : IManufacturerService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ManufacturerResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var spec = new QuerySpec<Manufacturer> { Includes = [nameof(Manufacturer.Models)], OrderBy = m => m.Name };

        var items = await manufacturers.ListAsync(spec, cancellationToken).ConfigureAwait(false);

        return Result<IReadOnlyList<ManufacturerResponse>>.Ok(items.Select(ToResponse).ToList());
    }

    /// <inheritdoc />
    public async Task<Result<ManufacturerResponse>> CreateAsync(ManufacturerRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var existing = await manufacturers.ListAsync(new QuerySpec<Manufacturer> { Filter = m => m.Name == name }, cancellationToken).ConfigureAwait(false);

        if (existing.Count > 0)
        {
            return Result<ManufacturerResponse>.Conflict("A manufacturer with that name already exists.");
        }

        var manufacturer = new Manufacturer { Name = name };

        await manufacturers.AddAsync(manufacturer, cancellationToken).ConfigureAwait(false);

        await manufacturers.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created manufacturer {ManufacturerId} ({ManufacturerName})", manufacturer.Id, manufacturer.Name);

        return Result<ManufacturerResponse>.Ok(ToResponse(manufacturer));
    }

    /// <inheritdoc />
    public async Task<Result<ManufacturerModelResponse>> CreateModelAsync(Guid manufacturerId, ManufacturerRequest request, CancellationToken cancellationToken)
    {
        var manufacturer = await manufacturers.GetByIdAsync(manufacturerId, null, cancellationToken).ConfigureAwait(false);

        if (manufacturer is null)
        {
            return Result<ManufacturerModelResponse>.NotFound("Manufacturer not found.");
        }

        var name = request.Name.Trim();

        var existing = await models.ListAsync(new QuerySpec<ManufacturerModel> { Filter = m => m.ManufacturerId == manufacturerId && m.Name == name }, cancellationToken).ConfigureAwait(false);

        if (existing.Count > 0)
        {
            return Result<ManufacturerModelResponse>.Conflict("That manufacturer already has a model with that name.");
        }

        var model = new ManufacturerModel { ManufacturerId = manufacturerId, Name = name };

        await models.AddAsync(model, cancellationToken).ConfigureAwait(false);

        await models.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Created model {ModelId} ({ModelName}) for manufacturer {ManufacturerId}", model.Id, model.Name, manufacturerId);

        return Result<ManufacturerModelResponse>.Ok(new ManufacturerModelResponse(model.Id, model.ManufacturerId, model.Name));
    }

    /// <summary>Maps a manufacturer to its response, models sorted by name.</summary>
    /// <param name="manufacturer">The entity (models loaded).</param>
    /// <returns>The response.</returns>
    private static ManufacturerResponse ToResponse(Manufacturer manufacturer)
    {
        var modelResponses = manufacturer.Models
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerModelResponse(m.Id, m.ManufacturerId, m.Name))
            .ToList();

        return new ManufacturerResponse(manufacturer.Id, manufacturer.Name, modelResponses);
    }
}
