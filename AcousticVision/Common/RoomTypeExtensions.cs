using AcousticVision.Models;

namespace AcousticVision.Common;

public static class RoomTypeExtensions
{
    public static string ToDisplayName(this RoomType roomType)
    {
        return roomType switch
        {
            RoomType.LectureRoom => "Лекционная аудитория",
            RoomType.MeetingRoom => "Переговорная",
            RoomType.Office => "Офис",
            RoomType.DispatchRoom => "Диспетчерская",
            RoomType.Studio => "Студия",
            RoomType.Classroom => "Учебный класс",
            RoomType.ResidentialRoom => "Жилое помещение",
            _ => roomType.ToString()
        };
    }
}