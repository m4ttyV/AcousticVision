using AcousticVision.Common;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using static AcousticVision.ViewModels.RoomsViewModel;

namespace AcousticVision.ViewModels;

public partial class RoomsViewModel : ViewModelBase
{
    public sealed class RoomTypeOption
    {
        public RoomType Value { get; init; }
        public string DisplayName { get; init; } = string.Empty;
    }

    private readonly RoomModelService _roomModelService;

    [ObservableProperty]
    private ObservableCollection<RoomModel> _rooms = new();

    [ObservableProperty]
    private ObservableCollection<RoomTypeOption> _roomTypeOptions =
        new(RoomTypeRequirements
            .GetAllRoomTypes()
            .Select(x => new RoomTypeOption
            {
                Value = x,
                DisplayName = x.ToDisplayName()
            }));

    [ObservableProperty]
    private RoomTypeOption? _selectedRoomTypeOption;

    [ObservableProperty]
    private RoomModel? _selectedRoom;

    [ObservableProperty]
    private string _newRoomName = string.Empty;

    [ObservableProperty]
    private string _newLength = string.Empty;

    [ObservableProperty]
    private string _newWidth = string.Empty;

    [ObservableProperty]
    private string _newHeight = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public RoomsViewModel(RoomModelService roomModelService)
    {
        _roomModelService = roomModelService;
        SelectedRoomTypeOption = RoomTypeOptions.FirstOrDefault();
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            var items = await _roomModelService.GetAllAsync();
            Rooms = new ObservableCollection<RoomModel>(items);
            StatusMessage = $"Загружено помещений: {Rooms.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddRoomAsync()
    {
        StatusMessage = string.Empty;

        var name = NewRoomName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название помещения.";
            return;
        }

        if (SelectedRoomTypeOption is null)
        {
            StatusMessage = "Выберите тип помещения.";
            return;
        }

        if (!TryParsePositive(NewLength, out var length))
        {
            StatusMessage = "Введите корректную длину помещения (> 0).";
            return;
        }

        if (!TryParsePositive(NewWidth, out var width))
        {
            StatusMessage = "Введите корректную ширину помещения (> 0).";
            return;
        }

        if (!TryParsePositive(NewHeight, out var height))
        {
            StatusMessage = "Введите корректную высоту помещения (> 0).";
            return;
        }

        try
        {
            await _roomModelService.AddAsync(
                name,
                SelectedRoomTypeOption.Value,
                length,
                width,
                height);

            NewRoomName = string.Empty;
            NewLength = string.Empty;
            NewWidth = string.Empty;
            NewHeight = string.Empty;
            SelectedRoomTypeOption = RoomTypeOptions.FirstOrDefault();

            await LoadAsync();
            StatusMessage = "Помещение успешно добавлено.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedRoom is null)
        {
            StatusMessage = "Выберите помещение для удаления.";
            return;
        }

        try
        {
            await _roomModelService.DeleteAsync(SelectedRoom.Id);
            await LoadAsync();
            StatusMessage = "Помещение удалено.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    private static bool TryParsePositive(string? input, out double value)
    {
        var normalized = (input ?? string.Empty).Trim().Replace(',', '.');

        if (double.TryParse(
                normalized,
                System.Globalization.CultureInfo.InvariantCulture,
                out value))
        {
            return value > 0;
        }

        value = 0;
        return false;
    }
}