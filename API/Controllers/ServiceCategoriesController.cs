using API.Data;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class ServiceCategoriesController(AppDbContext context) : BaseApiController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceCategoryDto>>> GetServiceCategories()
    {
        var categories = await context.ServiceCategories
            .OrderBy(x => x.Name)
            .Select(x => new ServiceCategoryDto
            {
                Id = x.Id,
                Key = x.Key,
                Name = x.Name
            })
            .ToListAsync();

        return categories;
    }
}
