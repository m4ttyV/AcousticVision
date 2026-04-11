using AcousticVision.Models;
using AcousticVision.Services;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AcousticVision.ViewModels;

public partial class TexturesViewModel : ViewModelBase
{
    private readonly TextureService _textureService;

    [ObservableProperty]
    private ObservableCollection<Texture> _textures = new();

    [ObservableProperty]
    private Texture? _selectedTexture;

    [ObservableProperty]
    private string _newTextureName = string.Empty;

    [ObservableProperty]
    private string _newNoiseCancelation = string.Empty;

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
            var items = await _textureService.GetAllAsync();
            Textures = new ObservableCollection<Texture>(items);
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

        var normalized = (NewNoiseCancelation ?? string.Empty).Replace(',', '.');

        if (!double.TryParse(normalized, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            StatusMessage = "Введите корректное числовое значение коэффициента.";
            return;
        }

        if (value < 0 || value > 1)
        {
            StatusMessage = "Коэффициент звукопоглощения должен быть в диапазоне от 0 до 1.";
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
            await LoadAsync();
            StatusMessage = "Фактура удалена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }
}