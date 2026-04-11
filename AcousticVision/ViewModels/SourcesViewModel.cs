using System.Collections.ObjectModel;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcousticVision.ViewModels;

public partial class SourcesViewModel : ViewModelBase
{
    private readonly SoundSourceService _soundSourceService;

    [ObservableProperty]
    private ObservableCollection<SoundSource> _sources = new();

    [ObservableProperty]
    private SoundSource? _selectedSource;

    [ObservableProperty]
    private string _newSourceName = string.Empty;

    [ObservableProperty]
    private string _newVolume = string.Empty;

    [ObservableProperty]
    private string _newArticle = string.Empty;

    [ObservableProperty]
    private string _newProperties = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SourcesViewModel(SoundSourceService soundSourceService)
    {
        _soundSourceService = soundSourceService;
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
            var items = await _soundSourceService.GetAllAsync();
            Sources = new ObservableCollection<SoundSource>(items);
            StatusMessage = $"Загружено источников: {Sources.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddSourceAsync()
    {
        StatusMessage = string.Empty;

        var name = NewSourceName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название источника.";
            return;
        }

        var volumeText = (NewVolume ?? string.Empty).Replace(',', '.');
        if (!double.TryParse(volumeText, System.Globalization.CultureInfo.InvariantCulture, out var volume))
        {
            StatusMessage = "Введите корректное значение громкости.";
            return;
        }

        if (volume < 0)
        {
            StatusMessage = "Громкость не может быть отрицательной.";
            return;
        }

        double? article = null;
        var articleText = (NewArticle ?? string.Empty).Trim();

        if (!string.IsNullOrWhiteSpace(articleText))
        {
            articleText = articleText.Replace(',', '.');

            if (!double.TryParse(articleText, System.Globalization.CultureInfo.InvariantCulture, out var parsedArticle))
            {
                StatusMessage = "Введите корректное значение Article.";
                return;
            }

            if (parsedArticle < 0 || parsedArticle > 1)
            {
                StatusMessage = "Article должен быть в диапазоне от 0 до 1.";
                return;
            }

            article = parsedArticle;
        }

        try
        {
            await _soundSourceService.AddAsync(name, volume, article, NewProperties);

            NewSourceName = string.Empty;
            NewVolume = string.Empty;
            NewArticle = string.Empty;
            NewProperties = string.Empty;

            await LoadAsync();
            StatusMessage = "Источник успешно добавлен.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedSource is null)
        {
            StatusMessage = "Выберите источник для удаления.";
            return;
        }

        try
        {
            await _soundSourceService.DeleteAsync(SelectedSource.Id);
            await LoadAsync();
            StatusMessage = "Источник удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }
}
