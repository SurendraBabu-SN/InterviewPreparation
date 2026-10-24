using Microsoft.EntityFrameworkCore;
using WebAPIPreparation.Models;

namespace WebAPIPreparation.Services
{
    public interface ICustomerService
    {
        Task<List<CustomerResponse>> GetAllAsync(CancellationToken ct);
        Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken ct);
        Task<CustomerResponse> CreateAsync(CreateCustomerRequest req, CancellationToken ct);
        Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest req, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
    }

    public class CustomerService(AppDbContext db) : ICustomerService
    {
        public Task<List<CustomerResponse>> GetAllAsync(CancellationToken ct) =>
            db.Customers.AsNoTracking()
              .Select(c => new CustomerResponse(c.Id, c.CustomerName, c.City, c.State, c.Country))
              .ToListAsync(ct);

        public Task<CustomerResponse?> GetByIdAsync(int id, CancellationToken ct) =>
            db.Customers.AsNoTracking()
              .Where(c => c.Id == id)
              .Select(c => new CustomerResponse(c.Id, c.CustomerName, c.City, c.State, c.Country))
              .FirstOrDefaultAsync(ct);

        public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest r, CancellationToken ct)
        {
            var entity = new Customer
            {
                Id = r.Id,
                CustomerName = r.CustomerName,
                City = r.City,
                State = r.State,
                Country = r.Country
            };
            db.Customers.Add(entity);
            await db.SaveChangesAsync(ct);
            return new CustomerResponse(entity.Id, entity.CustomerName, entity.City, entity.State, entity.Country);
        }

        public async Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest r, CancellationToken ct)
        {
            var entity = await db.Customers.FindAsync([id], ct);
            if (entity is null) return null;
            entity.CustomerName = r.CustomerName;
            entity.City = r.City; entity.State = r.State; entity.Country = r.Country;
            await db.SaveChangesAsync(ct);
            return new CustomerResponse(entity.Id, entity.CustomerName, entity.City, entity.State, entity.Country);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
            await db.Customers.Where(c => c.Id == id).ExecuteDeleteAsync(ct) > 0;
    }
}
