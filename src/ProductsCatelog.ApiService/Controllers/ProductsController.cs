using Microsoft.AspNetCore.Mvc;
using ProductsCatelog.ApiService.Models;
using ProductsCatelog.ApiService.Services;

namespace ProductsCatelog.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;

    public ProductsController(IProductRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        return Ok(items);
    }
}
