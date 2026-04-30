using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AcousticVision.ViewModels;

public partial class TexturesViewModel : ViewModelBase
{
    private readonly TextureService _textureService;
    private List<Texture> _allTextures = new();

    [ObservableProperty]
    private ObservableCollection<Texture> _textures = new();

    [ObservableProperty]
    private Texture? _selectedTexture;

    [ObservableProperty]
    private string _newTextureName = string.Empty;

    [ObservableProperty]
    private string _newNoiseCancelation = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _editTextureName = string.Empty;

    [ObservableProperty]
    private string _editNoiseCancelation = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public TexturesViewModel(TextureService textureService)
    {
        _textureService = textureService;
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
            _allTextures = await _textureService.GetAllAsync();
            ApplyFilter();
            StatusMessage = $"Загружено фактур: {Textures.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddTextureAsync()
    {
        StatusMessage = string.Empty;

        var name = NewTextureName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название фактуры.";
            return;
        }

        if (!TryParseFactor(NewNoiseCancelation, out var value))
        {
            StatusMessage = "Введите корректное числовое значение коэффициента в диапазоне от 0 до 1.";
            return;
        }

        try
        {
            await _textureService.AddAsync(name, value);
            NewTextureName = string.Empty;
            NewNoiseCancelation = string.Empty;
            await LoadAsync();
            StatusMessage = "Фактура успешно добавлена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateSelectedAsync()
    {
        if (SelectedTexture is null)
        {
            StatusMessage = "Выберите фактуру для изменения.";
            return;
        }

        var name = EditTextureName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название фактуры для изменения.";
            return;
        }

        if (!TryParseFactor(EditNoiseCancelation, out var value))
        {
            StatusMessage = "Введите корректное числовое значение коэффициента в диапазоне от 0 до 1.";
            return;
        }

        try
        {
            var selectedId = SelectedTexture.Id;
            await _textureService.UpdateAsync(selectedId, name, value);
            await LoadAsync();
            SelectedTexture = Textures.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = "Фактура успешно изменена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при изменении: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedTexture = null;
        EditTextureName = string.Empty;
        EditNoiseCancelation = string.Empty;
        StatusMessage = "Выбор фактуры сброшен.";
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedTexture is null)
        {
            StatusMessage = "Выберите фактуру для удаления.";
            return;
        }

        try
        {
            await _textureService.DeleteAsync(SelectedTexture.Id);
            SelectedTexture = null;
            EditTextureName = string.Empty;
            EditNoiseCancelation = string.Empty;
            await LoadAsync();
            StatusMessage = "Фактура удалена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    partial void OnSelectedTextureChanged(Texture? value)
    {
        if (value is null)
        {
            EditTextureName = string.Empty;
            EditNoiseCancelation = string.Empty;
            return;
        }

        EditTextureName = value.Name;
        EditNoiseCancelation = value.NoiseCancelation.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<Texture> filtered = _allTextures;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.NoiseCancelation.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    .Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        Textures = new ObservableCollection<Texture>(filtered);

        if (SelectedTexture is not null)
        {
            SelectedTexture = Textures.FirstOrDefault(x => x.Id == SelectedTexture.Id);
        }
    }

    private static bool TryParseFactor(string? input, out double value)
    {
        var normalized = (input ?? string.Empty).Replace(',', '.');
        if (double.TryParse(normalized, System.Globalization.CultureInfo.InvariantCulture, out value))
        {
            return value >= 0 && value <= 1;
        }

        value = 0;
        return false;
    }
}
