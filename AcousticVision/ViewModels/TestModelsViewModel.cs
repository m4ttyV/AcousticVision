using AcousticVision.Common;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Collections.ObjectModel;
using static AcousticVision.ViewModels.TestModelsViewModel;

namespace AcousticVision.ViewModels;

public partial class TestModelsViewModel : ViewModelBase
{
    public sealed class AnalysisMethodOption
    {
        public AnalysisMethod Value { get; init; }
        public string DisplayName { get; init; } = string.Empty;
    }

    private readonly TestModelService _testModelService;
    private readonly RoomModelService _roomModelService;
    private readonly SoundSourceService _soundSourceService;
    private readonly SoundReceiverService _soundReceiverService;

    [ObservableProperty]
    private ObservableCollection<TestModel> _testModels = new();

    [ObservableProperty]
    private ObservableCollection<RoomModel> _rooms = new();

    [ObservableProperty]
    private ObservableCollection<SoundSource> _sources = new();

    [ObservableProperty]
    private ObservableCollection<SoundReceiver> _receivers = new();

    [ObservableProperty]
    private ObservableCollection<AnalysisMethodOption> _analysisMethodOptions =
        new(Enum.GetValues<AnalysisMethod>()
            .Select(x => new AnalysisMethodOption
            {
                Value = x,
                DisplayName = x.ToDisplayName()
            }));

    [ObservableProperty]
    private TestModel? _selectedTestModel;

    [ObservableProperty]
    private RoomModel? _selectedRoom;

    [ObservableProperty]
    private SoundSource? _selectedSource;

    [ObservableProperty]
    private SoundReceiver? _selectedReceiver;

    [ObservableProperty]
    private AnalysisMethodOption? _selectedAnalysisMethodOption;

    [ObservableProperty]
    private string _newSourceLocation = string.Empty;

    [ObservableProperty]
    private string _newReceiverLocation = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public TestModelsViewModel(
        TestModelService testModelService,
        RoomModelService roomModelService,
        SoundSourceService soundSourceService,
        SoundReceiverService soundReceiverService)
    {
        _testModelService = testModelService;
        _roomModelService = roomModelService;
        _soundSourceService = soundSourceService;
        _soundReceiverService = soundReceiverService;

        SelectedAnalysisMethodOption = AnalysisMethodOptions.FirstOrDefault();
    }

    public async Task InitializeAsync()
    {
        await LoadLookupsAsync();
        await LoadTestModelsAsync();
    }

    private async Task LoadLookupsAsync()
    {
        var rooms = await _roomModelService.GetAllAsync();
        var sources = await _soundSourceService.GetAllAsync();
        var receivers = await _soundReceiverService.GetAllAsync();

        Rooms = new ObservableCollection<RoomModel>(rooms.OrderBy(x => x.Id));
        Sources = new ObservableCollection<SoundSource>(sources.OrderBy(x => x.Id));
        Receivers = new ObservableCollection<SoundReceiver>(receivers.OrderBy(x => x.Id));

        if (SelectedRoom is null && Rooms.Count > 0)
            SelectedRoom = Rooms[0];

        if (SelectedSource is null && Sources.Count > 0)
            SelectedSource = Sources[0];

        if (SelectedReceiver is null && Receivers.Count > 0)
            SelectedReceiver = Receivers[0];

        if (SelectedAnalysisMethodOption is null && AnalysisMethodOptions.Count > 0)
            SelectedAnalysisMethodOption = AnalysisMethodOptions[0];
    }

    [RelayCommand]
    private async Task LoadTestModelsAsync()
    {
        try
        {
            var items = await _testModelService.GetAllAsync();
            TestModels = new ObservableCollection<TestModel>(items.OrderBy(x => x.Id));
            StatusMessage = $"Загружено тестовых моделей: {TestModels.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddTestModelAsync()
    {
        StatusMessage = string.Empty;

        if (SelectedRoom is null)
        {
            StatusMessage = "Выберите помещение.";
            return;
        }

        if (SelectedSource is null)
        {
            StatusMessage = "Выберите источник.";
            return;
        }

        if (SelectedReceiver is null)
        {
            StatusMessage = "Выберите приёмник.";
            return;
        }

        if (SelectedAnalysisMethodOption is null)
        {
            StatusMessage = "Выберите метод расчёта.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewSourceLocation))
        {
            StatusMessage = "Введите координаты источника.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewReceiverLocation))
        {
            StatusMessage = "Введите координаты приёмника.";
            return;
        }

        try
        {
            await _testModelService.AddAsync(
                SelectedRoom.Id,
                SelectedSource.Id,
                SelectedReceiver.Id,
                NewSourceLocation,
                NewReceiverLocation,
                SelectedAnalysisMethodOption.Value);

            NewSourceLocation = string.Empty;
            NewReceiverLocation = string.Empty;
            SelectedAnalysisMethodOption = AnalysisMethodOptions.FirstOrDefault();

            await LoadTestModelsAsync();
            StatusMessage = "Тестовая модель успешно добавлена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedTestModel is null)
        {
            StatusMessage = "Выберите тестовую модель для удаления.";
            return;
        }

        try
        {
            await _testModelService.DeleteAsync(SelectedTestModel.Id);
            await LoadTestModelsAsync();
            StatusMessage = "Тестовая модель удалена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }
}