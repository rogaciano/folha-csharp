using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RhFolha.Domain.Companies;
using RhFolha.Domain.Security;
using RhFolha.Infrastructure.Persistence;

namespace RhFolha.Api.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(RhFolhaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var companies = await dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.LegalName)
            .Select(company => new CompanyResponse(
                company.Id,
                company.LegalName,
                company.TradeName,
                company.DocumentNumber,
                company.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    [Authorize(Roles = RoleGroups.AdminOnly)]
    [HttpPost]
    public async Task<IActionResult> Post(CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var company = new Company(request.LegalName, request.DocumentNumber);
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = company.Id }, new CompanyResponse(
            company.Id,
            company.LegalName,
            company.TradeName,
            company.DocumentNumber,
            company.IsActive));
    }
}

public sealed record CreateCompanyRequest(string LegalName, string DocumentNumber);
public sealed record CompanyResponse(Guid Id, string LegalName, string? TradeName, string DocumentNumber, bool IsActive);
