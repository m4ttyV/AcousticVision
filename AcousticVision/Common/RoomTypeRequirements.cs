using AcousticVision.Models;

namespace AcousticVision.Common;

public sealed class RoomTypeRequirement
{
    public RoomType RoomType { get; init; }
    public double MinRt60 { get; init; }
    public double MaxRt60 { get; init; }
    public string Description { get; init; } = string.Empty;
}

public static class RoomTypeRequirements
{
    private static readonly Dictionary<RoomType, RoomTypeRequirement> _requirements = new()
    {
        [RoomType.LectureRoom] = new RoomTypeRequirement
        {
            RoomType = RoomType.LectureRoom,
            MinRt60 = 0.6,
            MaxRt60 = 1.0,
            Description = "Для лекционных помещений важна высокая разборчивость речи при умеренном времени реверберации."
        },
        [RoomType.MeetingRoom] = new RoomTypeRequirement
        {
            RoomType = RoomType.MeetingRoom,
            MinRt60 = 0.4,
            MaxRt60 = 0.8,
            Description = "Для переговорных помещений предпочтительно пониженное время реверберации."
        },
        [RoomType.Office] = new RoomTypeRequirement
        {
            RoomType = RoomType.Office,
            MinRt60 = 0.4,
            MaxRt60 = 0.8,
            Description = "Для офисных помещений желательно ограничивать реверберацию для комфортной речи."
        },
        [RoomType.DispatchRoom] = new RoomTypeRequirement
        {
            RoomType = RoomType.DispatchRoom,
            MinRt60 = 0.3,
            MaxRt60 = 0.6,
            Description = "Для диспетчерских помещений приоритетом является максимальная разборчивость речи."
        },
        [RoomType.Studio] = new RoomTypeRequirement
        {
            RoomType = RoomType.Studio,
            MinRt60 = 0.2,
            MaxRt60 = 0.5,
            Description = "Для студий требуется низкое время реверберации и минимизация лишних отражений."
        },
        [RoomType.Classroom] = new RoomTypeRequirement
        {
            RoomType = RoomType.Classroom,
            MinRt60 = 0.5,
            MaxRt60 = 0.9,
            Description = "Для учебных классов требуется умеренное время реверберации для хорошей разборчивости речи."
        },
        [RoomType.ResidentialRoom] = new RoomTypeRequirement
        {
            RoomType = RoomType.ResidentialRoom,
            MinRt60 = 0.4,
            MaxRt60 = 0.8,
            Description = "Для жилых помещений важны комфортность звучания и приемлемая разборчивость речи без выраженной гулкости."
        }
    };

    public static RoomTypeRequirement Get(RoomType roomType)
    {
        return _requirements[roomType];
    }

    public static IReadOnlyList<RoomType> GetAllRoomTypes()
    {
        return Enum.GetValues<RoomType>();
    }
}