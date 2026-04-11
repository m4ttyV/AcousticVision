using AcousticVision.Common;
using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class TestModelService
{
    private readonly AppDbContext _dbContext;

    public TestModelService(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<List<TestModel>> GetAllAsync() =>
        _dbContext.TestModels
            .Include(x => x.Room)
            .Include(x => x.Source)
            .Include(x => x.Receiver)
            .OrderBy(x => x.Id)
            .ToListAsync();

    public async Task AddAsync(int roomId, int sourceId, int receiverId, string sourceLocation, string receiverLocation)
    {
        var room = await _dbContext.RoomModels.FirstOrDefaultAsync(x => x.Id == roomId);
        if (room is null)
            throw new InvalidOperationException("Помещение не найдено.");

        if (!CoordinateHelper.TryParsePoint3D(sourceLocation, out var sourcePoint))
            throw new InvalidOperationException("Координаты источника должны быть в формате (x; y; z).");

        if (!CoordinateHelper.TryParsePoint3D(receiverLocation, out var receiverPoint))
            throw new InvalidOperationException("Координаты приёмника должны быть в формате (x; y; z).");

        if (!CoordinateHelper.IsInsideRoom(sourcePoint, room.Length, room.Width, room.Height))
            throw new InvalidOperationException("Координаты источника выходят за границы помещения.");

        if (!CoordinateHelper.IsInsideRoom(receiverPoint, room.Length, room.Width, room.Height))
            throw new InvalidOperationException("Координаты приёмника выходят за границы помещения.");

        _dbContext.TestModels.Add(new TestModel
        {
            RoomId = roomId,
            SourceId = sourceId,
            ReceiverId = receiverId,
            SourceLocation = sourceLocation.Trim(),
            ReceiverLocation = receiverLocation.Trim()
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _dbContext.TestModels.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return;
        _dbContext.TestModels.Remove(item);
        await _dbContext.SaveChangesAsync();
    }
}
