using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcousticVision.ViewModels;

public partial class SourcesViewModel : ViewModelBase
{
    private readonly SoundSourceService _soundSourceService;
    private List<SoundSource> _allSources = new();

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
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _editSourceName = string.Empty;

    [ObservableProperty]
    private string _editVolume = string.Empty;

    [ObservableProperty]
    private string _editArticle = string.Empty;

    [ObservableProperty]
    private string _editProperties = string.Empty;

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
            _allSources = (await _soundSourceService.GetAllAsync())
                .OrderBy(x => x.Id)
                .ToList();
            ApplyFilter();
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

        if (!TryParseNonNegative(NewVolume, out var volume))
        {
            StatusMessage = "Введите корректное значение громкости.";
            return;
        }

        if (!TryParseArticle(NewArticle, out var article, out var articleError))
        {
            StatusMessage = articleError;
            return;
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
    private async Task UpdateSelectedAsync()
    {
        if (SelectedSource is null)
        {
            StatusMessage = "Выберите источник для изменения.";
            return;
        }

        var name = EditSourceName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название источника для изменения.";
            return;
        }

        if (!TryParseNonNegative(EditVolume, out var volume))
        {
            StatusMessage = "Введите корректное значение громкости.";
            return;
        }

        if (!TryParseArticle(EditArticle, out var article, out var articleError))
        {
            StatusMessage = articleError;
            return;
        }

        try
        {
            var selectedId = SelectedSource.Id;
            await _soundSourceService.UpdateAsync(selectedId, name, volume, article, EditProperties);
            await LoadAsync();
            SelectedSource = Sources.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = "Источник успешно изменён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при изменении: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedSource = null;
        EditSourceName = string.Empty;
        EditVolume = string.Empty;
        EditArticle = string.Empty;
        EditProperties = string.Empty;
        StatusMessage = "Выбор источника сброшен.";
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
            SelectedSource = null;
            EditSourceName = string.Empty;
            EditVolume = string.Empty;
            EditArticle = string.Empty;
            EditProperties = string.Empty;
            await LoadAsync();
            StatusMessage = "Источник удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    partial void OnSelectedSourceChanged(SoundSource? value)
    {
        if (value is null)
        {
            EditSourceName = string.Empty;
            EditVolume = string.Empty;
            EditArticle = string.Empty;
            EditProperties = string.Empty;
            return;
        }

        EditSourceName = value.Name;
        EditVolume = value.Volume.ToString(System.Globalization.CultureInfo.InvariantCulture);
        EditArticle = value.Article?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        EditProperties = value.Properties ?? string.Empty;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<SoundSource> filtered = _allSources;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Properties?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                x.Volume.ToString(System.Globalization.CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Article?.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    .Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Sources = new ObservableCollection<SoundSource>(filtered);

        if (SelectedSource is not null)
        {
            SelectedSource = Sources.FirstOrDefault(x => x.Id == SelectedSource.Id);
        }
    }

    private static bool TryParseNonNegative(string? input, out double value)
    {
        var normalized = (input ?? string.Empty).Replace(',', '.');
        if (double.TryParse(normalized, System.Globalization.CultureInfo.InvariantCulture, out value))
        {
            return value >= 0;
        }

        value = 0;
        return false;
    }

    private static bool TryParseArticle(string? input, out double? article, out string error)
    {
        article = null;
        error = string.Empty;
        var articleText = (input ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(articleText))
        {
            return true;
        }

        articleText = articleText.Replace(',', '.');

        if (!double.TryParse(articleText, System.Globalization.CultureInfo.InvariantCulture, out var parsedArticle))
        {
            error = "Введите корректное значение Article.";
            return false;
        }

        if (parsedArticle < 0 || parsedArticle > 1)
        {
            error = "Article должен быть в диапазоне от 0 до 1.";
            return false;
        }

        article = parsedArticle;
        return true;
    }
}
