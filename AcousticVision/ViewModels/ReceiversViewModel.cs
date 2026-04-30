using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcousticVision.ViewModels;

public partial class ReceiversViewModel : ViewModelBase
{
    private readonly SoundReceiverService _soundReceiverService;
    private List<SoundReceiver> _allReceivers = new();

    [ObservableProperty]
    private ObservableCollection<SoundReceiver> _receivers = new();

    [ObservableProperty]
    private SoundReceiver? _selectedReceiver;

    [ObservableProperty]
    private string _newReceiverName = string.Empty;

    [ObservableProperty]
    private string _newProperties = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _editReceiverName = string.Empty;

    [ObservableProperty]
    private string _editProperties = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ReceiversViewModel(SoundReceiverService soundReceiverService)
    {
        _soundReceiverService = soundReceiverService;
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
            _allReceivers = (await _soundReceiverService.GetAllAsync())
                .OrderBy(x => x.Id)
                .ToList();
            ApplyFilter();
            StatusMessage = $"Загружено приёмников: {Receivers.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddReceiverAsync()
    {
        StatusMessage = string.Empty;

        var name = NewReceiverName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название приёмника.";
            return;
        }

        try
        {
            await _soundReceiverService.AddAsync(name, NewProperties);

            NewReceiverName = string.Empty;
            NewProperties = string.Empty;

            await LoadAsync();
            StatusMessage = "Приёмник успешно добавлен.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateSelectedAsync()
    {
        if (SelectedReceiver is null)
        {
            StatusMessage = "Выберите приёмник для изменения.";
            return;
        }

        var name = EditReceiverName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название приёмника для изменения.";
            return;
        }

        try
        {
            var selectedId = SelectedReceiver.Id;
            await _soundReceiverService.UpdateAsync(selectedId, name, EditProperties);
            await LoadAsync();
            SelectedReceiver = Receivers.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = "Приёмник успешно изменён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при изменении: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedReceiver = null;
        EditReceiverName = string.Empty;
        EditProperties = string.Empty;
        StatusMessage = "Выбор приёмника сброшен.";
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedReceiver is null)
        {
            StatusMessage = "Выберите приёмник для удаления.";
            return;
        }

        try
        {
            await _soundReceiverService.DeleteAsync(SelectedReceiver.Id);
            SelectedReceiver = null;
            EditReceiverName = string.Empty;
            EditProperties = string.Empty;
            await LoadAsync();
            StatusMessage = "Приёмник удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    partial void OnSelectedReceiverChanged(SoundReceiver? value)
    {
        if (value is null)
        {
            EditReceiverName = string.Empty;
            EditProperties = string.Empty;
            return;
        }

        EditReceiverName = value.Name;
        EditProperties = value.Properties ?? string.Empty;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<SoundReceiver> filtered = _allReceivers;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Properties?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        Receivers = new ObservableCollection<SoundReceiver>(filtered);

        if (SelectedReceiver is not null)
        {
            SelectedReceiver = Receivers.FirstOrDefault(x => x.Id == SelectedReceiver.Id);
        }
    }
}
