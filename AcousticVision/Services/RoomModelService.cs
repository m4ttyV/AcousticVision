using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class RoomModelService
{
    private readonly AppDbContext _dbContext;

    public RoomModelService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoomModel>> GetAllAsync()
    {
        return await _dbContext.RoomModels
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<RoomModel> AddAsync(
        string name,
        RoomType roomType,
        double length,
        double width,
        double height)
    {
        var room = new RoomModel
        {
            Name = name.Trim(),
            RoomType = roomType,
            Length = length,
            Width = width,
            Height = height
        };

        _dbContext.RoomModels.Add(room);
        await _dbContext.SaveChangesAsync();

        return room;
    }

    public async Task UpdateAsync(
        int id,
        string name,
        RoomType roomType,
        double length,
        double width,
        double height)
    {
        var room = await _dbContext.RoomModels.FirstOrDefaultAsync(x => x.Id == id);
        if (room is null)
            throw new InvalidOperationException("Помещение не найдено.");

        room.Name = name.Trim();
        room.RoomType = roomType;
        room.Length = length;
        room.Width = width;
        room.Height = height;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _dbContext.RoomModels.FirstOrDefaultAsync(x => x.Id == id);
        if (room is null)
            return;

        _dbContext.RoomModels.Remove(room);
        await _dbContext.SaveChangesAsync();
    }
}
