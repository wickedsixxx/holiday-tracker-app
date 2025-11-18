using System;
using HolidayTrackerApp.Domain;
using HolidayTrackerApp.Domain.Entities;
using HolidayTrackerApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolidayTrackerApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;
    public EmployeesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Employees.AsNoTracking().ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var emp = await _db.Employees.FindAsync(id);
        return emp is null ? NotFound() : Ok(emp);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Employee e)
    {
        // Gerekirse Id otomatik ver:
        if (e.Id == Guid.Empty) e.Id = Guid.NewGuid();

        _db.Employees.Add(e);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = e.Id }, e);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Employee e)
    {
        if (id != e.Id) return BadRequest();

        var exists = await _db.Employees.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        _db.Entry(e).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp is null) return NotFound();

        _db.Employees.Remove(emp);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
