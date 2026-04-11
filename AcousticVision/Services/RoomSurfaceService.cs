using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class RoomSurfaceService
{
    private readonly AppDbContext _dbContext;

    public RoomSurfaceService(AppDbContext dbContext) => _dbContext = dbContext;

    public static readonly string[] AllowedPositions =
    {
        "floor", "ceiling", "north", "south", "east", "west"
    };

    public Task<List<RoomSurface>> GetAllAsync() =>
        _dbContext.RoomSurfaces
            .Include(x => x.Room)
            .Include(x => x.Material)
            .Include(x => x.Texture)
            .OrderBy(x => x.RoomId)
            .ThenBy(x => x.Position)
            .ToListAsync();

    public async Task AddOrUpdateAsync(int roomId, string position, int materialId, int textureId)
    {
        var normalized = position.Trim().ToLowerInvariant();
        var existing = await _dbContext.RoomSurfaces
            .FirstOrDefaultAsync(x => x.RoomId == roomId && x.Position == normalized);

        if (existing is not null)
        {
            existing.MaterialId = materialId;
            existing.TextureId = textureId;
        }
        else
        {
            _dbContext.RoomSurfaces.Add(new RoomSurface
            {
                RoomId = roomId,
                Position = normalized,
                MaterialId = materialId,
                TextureId = textureId
            });
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int roomId, string position)
    {
        var normalized = position.Trim().ToLowerInvariant();
        var item = await _dbContext.RoomSurfaces
            .FirstOrDefaultAsync(x => x.RoomId == roomId && x.Position == normalized);
        if (item is null) return;
        _dbContext.RoomSurfaces.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
