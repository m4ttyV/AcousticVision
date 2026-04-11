using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class RoomModelService
{
    private readonly AppDbContext _dbContext;

    public RoomModelService(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<List<RoomModel>> GetAllAsync() =>
        _dbContext.RoomModels.OrderBy(x => x.Name).ToListAsync();

    public async Task AddAsync(string name, double length, double width, double height)
    {
        _dbContext.RoomModels.Add(new RoomModel
        {
            Name = name.Trim(),
            Length = length,
            Width = width,
            Height = height
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _dbContext.RoomModels.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return;
        _dbContext.RoomModels.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
