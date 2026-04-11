using System.Collections.ObjectModel;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcousticVision.ViewModels;

public partial class ReceiversViewModel : ViewModelBase
{
    private readonly SoundReceiverService _soundReceiverService;

    [ObservableProperty]
    private ObservableCollection<SoundReceiver> _receivers = new();

    [ObservableProperty]
    private SoundReceiver? _selectedReceiver;

    [ObservableProperty]
    private string _newReceiverName = string.Empty;

    [ObservableProperty]
    private string _newProperties = string.Empty;

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
            var items = await _soundReceiverService.GetAllAsync();
            Receivers = new ObservableCollection<SoundReceiver>(items);
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
            await LoadAsync();
            StatusMessage = "Приёмник удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }
}
