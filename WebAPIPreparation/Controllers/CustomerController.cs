using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIPreparation.Auth;
using WebAPIPreparation.Models;
using WebAPIPreparation.Services;

namespace WebAPIPreparation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController(ICustomerService service) : ControllerBase
    {
        [HttpGet]
        [Authorize(Policy = Permissions.CustomersView)]
        public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));

        [HttpPost]
        [Authorize(Policy = Permissions.CustomersEdit)]
        public async Task<IActionResult> Create(CreateCustomerRequest req, CancellationToken ct) { /* ... */ }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Permissions.CustomersEdit)]
        public async Task<IActionResult> Update(int id, UpdateCustomerRequest req, CancellationToken ct) { /* ... */ }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Permissions.CustomersDelete)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct) { /* ... */ }
    }
}
