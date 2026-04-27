using AcousticVision.Data;
using AcousticVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AcousticVision.Services;

public class TestModelService
{
    private readonly AppDbContext _dbContext;

    public TestModelService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TestModel>> GetAllAsync()
    {
        return await _dbContext.TestModels
            .Include(x => x.Room)
            .Include(x => x.Source)
            .Include(x => x.Receiver)
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<TestModel> AddAsync(
        int roomId,
        int sourceId,
        int receiverId,
        string sourceLocation,
        string receiverLocation,
        AnalysisMethod analysisMethod)
    {
        var model = new TestModel
        {
            RoomId = roomId,
            SourceId = sourceId,
            ReceiverId = receiverId,
            SourceLocation = sourceLocation.Trim(),
            ReceiverLocation = receiverLocation.Trim(),
            AnalysisMethod = analysisMethod
        };

        _dbContext.TestModels.Add(model);
        await _dbContext.SaveChangesAsync();

        return model;
    }

    public async Task DeleteAsync(int id)
    {
        var model = await _dbContext.TestModels.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null)
            return;

        _dbContext.TestModels.Remove(model);
        await _dbContext.SaveChangesAsync();
    }
}