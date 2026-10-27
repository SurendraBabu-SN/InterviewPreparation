using Microsoft.EntityFrameworkCore;
using WebAPIPreparation.Models;
using WebAPIPreparation.Utility;

namespace WebAPIPreparation.Services;

public interface ICountryService
{
    Task<List<CountryResponse>> GetAllAsync(string? search, CancellationToken ct);
    Task<CountryResponse?> GetByIdAsync(long id, CancellationToken ct);
    Task<CountryResponse> CreateAsync(CreateCountryRequest request, CancellationToken ct);
    Task<CountryResponse?> UpdateAsync(long id, UpdateCountryRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(long id, CancellationToken ct);
}

public class CountryService(AppDbContext db) : ICountryService
{
    public async Task<List<CountryResponse>> GetAllAsync(string? search, CancellationToken ct)
    {
        var query = db.Countries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CountryName.Contains(search));

        return await query
            .OrderBy(c => c.CountryName)
            .Select(c => new CountryResponse(c.Id, c.CountryName))
            .ToListAsync(ct);
    }

    public Task<CountryResponse?> GetByIdAsync(long id, CancellationToken ct) =>
        db.Countries.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CountryResponse(c.Id, c.CountryName))
            .FirstOrDefaultAsync(ct);

    public async Task<CountryResponse> CreateAsync(CreateCountryRequest request, CancellationToken ct)
    {
        var name = request.CountryName.Trim();

        //if (await db.Countries.AnyAsync(c => c.CountryName == request.CountryName, ct))
        //    throw new ConflictException($"Country with Name {request.CountryName} already exists.");

        if (await db.Countries.AnyAsync(c => c.CountryName == name, ct))
            throw new ConflictException($"Country '{name}' already exists.");

        var entity = new Country { Id = request.Id, CountryName = name };
        db.Countries.Add(entity);
        await db.SaveChangesAsync(ct);

        return new CountryResponse(entity.Id, entity.CountryName);
    }

    public async Task<CountryResponse?> UpdateAsync(long id, UpdateCountryRequest request, CancellationToken ct)
    {
        var entity = await db.Countries.FindAsync([id], ct);
        if (entity is null) return null;                          // → 404 in controller

        var name = request.CountryName.Trim();
        if (await db.Countries.AnyAsync(c => c.CountryName == name && c.Id != id, ct))
            throw new ConflictException($"Country '{name}' already exists.");

        entity.CountryName = name;
        await db.SaveChangesAsync(ct);

        return new CountryResponse(entity.Id, entity.CountryName);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        // No foreign key in the DB, so we protect the data ourselves
        if (await db.States.AnyAsync(s => s.CountryId == id, ct))
            throw new ConflictException("This country still has states. Delete or move them first.");

        return await db.Countries.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
    }
}