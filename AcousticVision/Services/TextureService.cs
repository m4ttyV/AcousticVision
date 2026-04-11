using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class TextureService
{
    private readonly AppDbContext _dbContext;

    public TextureService(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<List<Texture>> GetAllAsync() =>
        _dbContext.Textures.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(string name, double noiseCancelation)
    {
        _dbContext.Textures.Add(new Texture
        {
            Name = name.Trim(),
            NoiseCancelation = noiseCancelation
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _dbContext.Textures.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return;
        _dbContext.Textures.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
