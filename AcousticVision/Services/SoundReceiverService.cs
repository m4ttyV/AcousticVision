using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class SoundReceiverService
{
    private readonly AppDbContext _dbContext;

    public SoundReceiverService(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<List<SoundReceiver>> GetAllAsync() =>
        _dbContext.SoundReceivers.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(string name, string? properties)
    {
        _dbContext.SoundReceivers.Add(new SoundReceiver
        {
            Name = name.Trim(),
            Properties = string.IsNullOrWhiteSpace(properties) ? null : properties.Trim()
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _dbContext.SoundReceivers.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return;
        _dbContext.SoundReceivers.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
