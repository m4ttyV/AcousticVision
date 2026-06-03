using AcousticVision.Common;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Collections.ObjectModel;
using static AcousticVision.ViewModels.RoomSurfacesViewModel;

namespace AcousticVision.ViewModels;

public partial class RoomSurfacesViewModel : ViewModelBase
{
    private readonly RoomSurfaceService _roomSurfaceService;
    private readonly RoomModelService _roomModelService;
    private readonly MaterialService _materialService;
    private readonly TextureService _textureService;
    private List<RoomSurface> _allSurfaces = new();

    [ObservableProperty] private ObservableCollection<RoomSurface> _surfaces = new();
    [ObservableProperty] private ObservableCollection<RoomModel> _rooms = new();
    [ObservableProperty] private ObservableCollection<Material> _materials = new();
    [ObservableProperty] private ObservableCollection<Texture> _textures = new();
    [ObservableProperty]
    private ObservableCollection<PositionOption> _positionOptions =
    new(RoomSurfaceService.AllowedPositions.Select(x => new PositionOption
    {
        Value = x,
        DisplayName = x.ToDisplayName()
    }));

    [ObservableProperty]
    private PositionOption? _selectedPositionOption;
    [ObservableProperty] private RoomSurface? _selectedSurface;
    [ObservableProperty] private RoomModel? _selectedRoom;
    [ObservableProperty] private Material? _selectedMaterial;
    [ObservableProperty] private Texture? _selectedTexture;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public RoomSurfacesViewModel(RoomSurfaceService roomSurfaceService, RoomModelService roomModelService, MaterialService materialService, TextureService textureService)
    {
        _roomSurfaceService = roomSurfaceService;
        _roomModelService = roomModelService;
        _materialService = materialService;
        _textureService = textureService;
    }

    public sealed class PositionOption
    {
        public string Value { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
    }

    public async Task InitializeAsync()
    {
        await LoadLookupsAsync();
        await LoadSurfacesAsync();
    }

    private async Task LoadLookupsAsync()
    {
        Rooms = new ObservableCollection<RoomModel>((await _roomModelService.GetAllAsync()).OrderBy(x => x.Id));
        Materials = new ObservableCollection<Material>((await _materialService.GetAllAsync()).OrderBy(x => x.Id));
        Textures = new ObservableCollection<Texture>((await _textureService.GetAllAsync()).OrderBy(x => x.Id));

        SelectedRoom ??= Rooms.FirstOrDefault();
        SelectedMaterial ??= Materials.FirstOrDefault();
        SelectedTexture ??= Textures.FirstOrDefault();
        SelectedPositionOption ??= PositionOptions.FirstOrDefault();
    }

    [RelayCommand]
    private async Task LoadSurfacesAsync()
    {
        try
        {
            var selectedKey = SelectedSurface is null
                ? null
                : $"{SelectedSurface.RoomId}:{SelectedSurface.Position}";

            var items = await _roomSurfaceService.GetAllAsync();
            _allSurfaces = items
                .OrderBy(x => x.Room != null ? x.Room.Name : string.Empty)
                .ThenBy(x => x.Position)
                .ToList();

            ApplyFilter();

            if (selectedKey is not null)
                SelectedSurface = Surfaces.FirstOrDefault(x => $"{x.RoomId}:{x.Position}" == selectedKey);

            StatusMessage = BuildLoadedMessage();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSurfaceAsync()
    {
        StatusMessage = string.Empty;

        if (SelectedRoom is null)
        {
            StatusMessage = "Выберите помещение.";
            return;
        }
        if (SelectedMaterial is null)
        {
            StatusMessage = "Выберите материал.";
            return;
        }
        if (SelectedTexture is null)
        {
            StatusMessage = "Выберите фактуру.";
            return;
        }
        if (SelectedPositionOption is null)
        {
            StatusMessage = "Выберите позицию.";
            return;
        }

        try
        {
            await _roomSurfaceService.AddOrUpdateAsync(SelectedRoom.Id, SelectedPositionOption.Value, SelectedMaterial.Id, SelectedTexture.Id);
            await LoadSurfacesAsync();
            StatusMessage = "Поверхность сохранена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        StatusMessage = BuildLoadedMessage();
    }

    partial void OnSearchTextChanged(string value)
    {
        var selectedKey = SelectedSurface is null
            ? null
            : $"{SelectedSurface.RoomId}:{SelectedSurface.Position}";

        ApplyFilter();

        if (selectedKey is not null)
            SelectedSurface = Surfaces.FirstOrDefault(x => $"{x.RoomId}:{x.Position}" == selectedKey);

        StatusMessage = BuildLoadedMessage();
    }

    private void ApplyFilter()
    {
        IEnumerable<RoomSurface> filtered = _allSurfaces;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.RoomId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.MaterialId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.TextureId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.Position.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.PositionDisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Room?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Material?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Texture?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Surfaces = new ObservableCollection<RoomSurface>(filtered);

        if (SelectedSurface is not null)
        {
            var selectedRoomId = SelectedSurface.RoomId;
            var selectedPosition = SelectedSurface.Position;
            SelectedSurface = Surfaces.FirstOrDefault(x =>
                x.RoomId == selectedRoomId &&
                x.Position == selectedPosition);
        }
    }

    private string BuildLoadedMessage()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return $"Загружено поверхностей: {Surfaces.Count}";

        return $"Найдено поверхностей: {Surfaces.Count} из {_allSurfaces.Count}";
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedSurface is null)
        {
            StatusMessage = "Выберите поверхность для удаления.";
            return;
        }

        try
        {
            await _roomSurfaceService.DeleteAsync(SelectedSurface.RoomId, SelectedSurface.Position);
            await LoadSurfacesAsync();
            StatusMessage = "Поверхность удалена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
    }
}
