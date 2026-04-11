using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class MaterialService
{
    private readonly AppDbContext _dbContext;

    public MaterialService(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<List<Material>> GetAllAsync() =>
        _dbContext.Materials.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(string name, double noiseCancelation)
    {
        _dbContext.Materials.Add(new Material
        {
            Name = name.Trim(),
            NoiseCancelation = noiseCancelation
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _dbContext.Materials.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return;
        _dbContext.Materials.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
