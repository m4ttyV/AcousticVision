using AcousticVision.Common;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

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
    private List<TestModel> _allTestModels = new();

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
    private string _searchText = string.Empty;

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

        SelectedRoom ??= Rooms.FirstOrDefault();
        SelectedSource ??= Sources.FirstOrDefault();
        SelectedReceiver ??= Receivers.FirstOrDefault();
        SelectedAnalysisMethodOption ??= AnalysisMethodOptions.FirstOrDefault();
    }

    [RelayCommand]
    private async Task LoadTestModelsAsync()
    {
        try
        {
            var selectedId = SelectedTestModel?.Id;
            var items = await _testModelService.GetAllAsync();
            _allTestModels = items.OrderBy(x => x.Id).ToList();

            ApplyFilter();

            if (selectedId is not null)
                SelectedTestModel = TestModels.FirstOrDefault(x => x.Id == selectedId.Value);

            StatusMessage = BuildLoadedMessage();
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

        if (!ValidateForm())
            return;

        try
        {
            var added = await _testModelService.AddAsync(
                SelectedRoom!.Id,
                SelectedSource!.Id,
                SelectedReceiver!.Id,
                NewSourceLocation,
                NewReceiverLocation,
                SelectedAnalysisMethodOption!.Value);

            await LoadTestModelsAsync();
            SelectedTestModel = TestModels.FirstOrDefault(x => x.Id == added.Id);
            StatusMessage = "Тестовая модель успешно добавлена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateSelectedAsync()
    {
        StatusMessage = string.Empty;

        if (SelectedTestModel is null)
        {
            StatusMessage = "Выберите тестовую модель для редактирования.";
            return;
        }

        if (!ValidateForm())
            return;

        try
        {
            var id = SelectedTestModel.Id;

            await _testModelService.UpdateAsync(
                id,
                SelectedRoom!.Id,
                SelectedSource!.Id,
                SelectedReceiver!.Id,
                NewSourceLocation,
                NewReceiverLocation,
                SelectedAnalysisMethodOption!.Value);

            await LoadTestModelsAsync();
            SelectedTestModel = TestModels.FirstOrDefault(x => x.Id == id);
            StatusMessage = "Изменения тестовой модели сохранены.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при сохранении изменений: {ex.Message}";
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
            SelectedTestModel = null;
            ClearFormFields(resetLookups: false);
            await LoadTestModelsAsync();
            StatusMessage = "Тестовая модель удалена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedTestModel = null;
        ClearFormFields(resetLookups: true);
        StatusMessage = "Форма очищена. Можно добавить новую тестовую модель.";
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        StatusMessage = BuildLoadedMessage();
    }

    partial void OnSelectedTestModelChanged(TestModel? value)
    {
        if (value is null)
            return;

        SelectedRoom = Rooms.FirstOrDefault(x => x.Id == value.RoomId) ?? value.Room;
        SelectedSource = Sources.FirstOrDefault(x => x.Id == value.SourceId) ?? value.Source;
        SelectedReceiver = Receivers.FirstOrDefault(x => x.Id == value.ReceiverId) ?? value.Receiver;
        SelectedAnalysisMethodOption = AnalysisMethodOptions.FirstOrDefault(x => x.Value == value.AnalysisMethod)
                                       ?? AnalysisMethodOptions.FirstOrDefault();
        NewSourceLocation = value.SourceLocation;
        NewReceiverLocation = value.ReceiverLocation;
    }

    partial void OnSearchTextChanged(string value)
    {
        var selectedId = SelectedTestModel?.Id;
        ApplyFilter();

        if (selectedId is not null)
            SelectedTestModel = TestModels.FirstOrDefault(x => x.Id == selectedId.Value);

        StatusMessage = BuildLoadedMessage();
    }

    private void ApplyFilter()
    {
        IEnumerable<TestModel> filtered = _allTestModels;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.Id.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (x.Room?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Source?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Receiver?.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                x.SourceLocation.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.ReceiverLocation.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.AnalysisMethodDisplayName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        TestModels = new ObservableCollection<TestModel>(filtered);

        if (SelectedTestModel is not null)
            SelectedTestModel = TestModels.FirstOrDefault(x => x.Id == SelectedTestModel.Id);
    }

    private string BuildLoadedMessage()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return $"Загружено тестовых моделей: {TestModels.Count}";

        return $"Найдено тестовых моделей: {TestModels.Count} из {_allTestModels.Count}";
    }

    private bool ValidateForm()
    {
        if (SelectedRoom is null)
        {
            StatusMessage = "Выберите помещение.";
            return false;
        }

        if (SelectedSource is null)
        {
            StatusMessage = "Выберите источник.";
            return false;
        }

        if (SelectedReceiver is null)
        {
            StatusMessage = "Выберите приёмник.";
            return false;
        }

        if (SelectedAnalysisMethodOption is null)
        {
            StatusMessage = "Выберите метод расчёта.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewSourceLocation))
        {
            StatusMessage = "Введите координаты источника.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NewReceiverLocation))
        {
            StatusMessage = "Введите координаты приёмника.";
            return false;
        }

        return true;
    }

    private void ClearFormFields(bool resetLookups)
    {
        NewSourceLocation = string.Empty;
        NewReceiverLocation = string.Empty;

        if (!resetLookups)
            return;

        SelectedRoom = Rooms.FirstOrDefault();
        SelectedSource = Sources.FirstOrDefault();
        SelectedReceiver = Receivers.FirstOrDefault();
        SelectedAnalysisMethodOption = AnalysisMethodOptions.FirstOrDefault();
    }
}
