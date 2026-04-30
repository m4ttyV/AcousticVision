using AcousticVision.Common;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AcousticVision.ViewModels;

public partial class RoomsViewModel : ViewModelBase
{
    public sealed class RoomTypeOption
    {
        public RoomType Value { get; init; }
        public string DisplayName { get; init; } = string.Empty;
    }

    private readonly RoomModelService _roomModelService;
    private List<RoomModel> _allRooms = new();

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
    private RoomTypeOption? _editRoomTypeOption;

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
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _editRoomName = string.Empty;

    [ObservableProperty]
    private string _editLength = string.Empty;

    [ObservableProperty]
    private string _editWidth = string.Empty;

    [ObservableProperty]
    private string _editHeight = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public RoomsViewModel(RoomModelService roomModelService)
    {
        _roomModelService = roomModelService;
        SelectedRoomTypeOption = RoomTypeOptions.FirstOrDefault();
        EditRoomTypeOption = RoomTypeOptions.FirstOrDefault();
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
            _allRooms = await _roomModelService.GetAllAsync();
            ApplyFilter();
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
    private async Task UpdateSelectedAsync()
    {
        if (SelectedRoom is null)
        {
            StatusMessage = "Выберите помещение для изменения.";
            return;
        }

        var name = EditRoomName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название помещения для изменения.";
            return;
        }

        if (EditRoomTypeOption is null)
        {
            StatusMessage = "Выберите тип помещения для изменения.";
            return;
        }

        if (!TryParsePositive(EditLength, out var length))
        {
            StatusMessage = "Введите корректную длину помещения (> 0).";
            return;
        }

        if (!TryParsePositive(EditWidth, out var width))
        {
            StatusMessage = "Введите корректную ширину помещения (> 0).";
            return;
        }

        if (!TryParsePositive(EditHeight, out var height))
        {
            StatusMessage = "Введите корректную высоту помещения (> 0).";
            return;
        }

        try
        {
            var selectedId = SelectedRoom.Id;
            await _roomModelService.UpdateAsync(
                selectedId,
                name,
                EditRoomTypeOption.Value,
                length,
                width,
                height);

            await LoadAsync();
            SelectedRoom = Rooms.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = "Помещение успешно изменено.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при изменении: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedRoom = null;
        EditRoomName = string.Empty;
        EditLength = string.Empty;
        EditWidth = string.Empty;
        EditHeight = string.Empty;
        EditRoomTypeOption = RoomTypeOptions.FirstOrDefault();
        StatusMessage = "Выбор помещения сброшен.";
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
            SelectedRoom = null;
            EditRoomName = string.Empty;
            EditLength = string.Empty;
            EditWidth = string.Empty;
            EditHeight = string.Empty;
            EditRoomTypeOption = RoomTypeOptions.FirstOrDefault();
            await LoadAsync();
            StatusMessage = "Помещение удалено.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    partial void OnSelectedRoomChanged(RoomModel? value)
    {
        if (value is null)
        {
            EditRoomName = string.Empty;
            EditLength = string.Empty;
            EditWidth = string.Empty;
            EditHeight = string.Empty;
            EditRoomTypeOption = RoomTypeOptions.FirstOrDefault();
            return;
        }

        EditRoomName = value.Name;
        EditLength = value.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
        EditWidth = value.Width.ToString(System.Globalization.CultureInfo.InvariantCulture);
        EditHeight = value.Height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        EditRoomTypeOption = RoomTypeOptions.FirstOrDefault(x => x.Value == value.RoomType);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<RoomModel> filtered = _allRooms;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.RoomTypeDisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.Length.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.Width.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.Height.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        Rooms = new ObservableCollection<RoomModel>(filtered);

        if (SelectedRoom is not null)
        {
            SelectedRoom = Rooms.FirstOrDefault(x => x.Id == SelectedRoom.Id);
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
