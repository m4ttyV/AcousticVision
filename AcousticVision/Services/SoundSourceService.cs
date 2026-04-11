using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class SoundSourceService
{
    private readonly AppDbContext _dbContext;

    public SoundSourceService(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<List<SoundSource>> GetAllAsync() =>
        _dbContext.SoundSources.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(string name, double volume, double? article, string? properties)
    {
        _dbContext.SoundSources.Add(new SoundSource
        {
            Name = name.Trim(),
            Volume = volume,
            Article = article,
            Properties = string.IsNullOrWhiteSpace(properties) ? null : properties.Trim()
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _dbContext.SoundSources.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return;
        _dbContext.SoundSources.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
