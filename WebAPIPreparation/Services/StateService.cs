

using WebAPIPreparation.Models;
using WebAPIPreparation.Utility;
using Microsoft.EntityFrameworkCore;

namespace WebAPIPreparation.Services
{
    public interface IStateService
    {
        Task<List<StateResponse>> GetAllAsync(long? countryId, CancellationToken ct);
        Task<StateResponse?> GetByIdAsync(long id, CancellationToken ct);
        Task<StateResponse> CreateAsync(CreateStateRequest request, CancellationToken ct);
        Task<StateResponse?> UpdateAsync(long id, UpdateStateRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);
    }

    public class StateService(AppDbContext db) : IStateService
    {
        // State has no navigation property to Country (no FK in the DB),
        // so a LEFT JOIN brings in the country name.
        private IQueryable<StateResponse> StatesWithCountry() =>
            from s in db.States.AsNoTracking()
            join c in db.Countries.AsNoTracking() on s.CountryId equals c.Id into gj
            from c in gj.DefaultIfEmpty()
            select new StateResponse(s.Id, s.StateName, s.CountryId, c != null ? c.CountryName : null);

        public async Task<List<StateResponse>> GetAllAsync(long? countryId, CancellationToken ct)
        {
            var query = StatesWithCountry();

            if (countryId.HasValue)                        // used for the State dropdown
                query = query.Where(s => s.CountryId == countryId.Value);

            return await query.OrderBy(s => s.StateName).ToListAsync(ct);
        }

        public Task<StateResponse?> GetByIdAsync(long id, CancellationToken ct) =>
            StatesWithCountry().FirstOrDefaultAsync(s => s.Id == id, ct);

        public async Task<StateResponse> CreateAsync(CreateStateRequest request, CancellationToken ct)
        {
            var name = request.StateName.Trim();

            if (await db.States.AnyAsync(s => s.Id == request.Id, ct))
                throw new ConflictException($"State with Id {request.Id} already exists.");

            await EnsureCountryExistsAsync(request.CountryId, ct);

            if (await db.States.AnyAsync(s => s.CountryId == request.CountryId && s.StateName == name, ct))
                throw new ConflictException($"State '{name}' already exists in this country.");

            db.States.Add(new State { Id = request.Id, StateName = name, CountryId = request.CountryId });
            await db.SaveChangesAsync(ct);

            return (await GetByIdAsync(request.Id, ct))!;
        }

        public async Task<StateResponse?> UpdateAsync(long id, UpdateStateRequest request, CancellationToken ct)
        {
            var entity = await db.States.FindAsync([id], ct);
            if (entity is null) return null;                          // → 404 in controller

            await EnsureCountryExistsAsync(request.CountryId, ct);

            var name = request.StateName.Trim();
            if (await db.States.AnyAsync(s => s.CountryId == request.CountryId && s.StateName == name && s.Id != id, ct))
                throw new ConflictException($"State '{name}' already exists in this country.");

            entity.StateName = name;
            entity.CountryId = request.CountryId;
            await db.SaveChangesAsync(ct);

            return await GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken ct) =>
            await db.States.Where(s => s.Id == id).ExecuteDeleteAsync(ct) > 0;

        private async Task EnsureCountryExistsAsync(long countryId, CancellationToken ct)
        {
            if (!await db.Countries.AnyAsync(c => c.Id == countryId, ct))
                throw new BadRequestException($"Country with Id {countryId} does not exist.");
        }
    }
}
