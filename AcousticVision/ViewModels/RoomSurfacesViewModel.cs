using AcousticVision.Common;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using static AcousticVision.ViewModels.RoomSurfacesViewModel;

namespace AcousticVision.ViewModels;

public partial class RoomSurfacesViewModel : ViewModelBase
{
    private readonly RoomSurfaceService _roomSurfaceService;
    private readonly RoomModelService _roomModelService;
    private readonly MaterialService _materialService;
    private readonly TextureService _textureService;

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
        Rooms = new ObservableCollection<RoomModel>(await _roomModelService.GetAllAsync());
        Materials = new ObservableCollection<Material>(await _materialService.GetAllAsync());
        Textures = new ObservableCollection<Texture>(await _textureService.GetAllAsync());

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
            Surfaces = new ObservableCollection<RoomSurface>(await _roomSurfaceService.GetAllAsync());
            StatusMessage = $"Загружено поверхностей: {Surfaces.Count}";
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
