using System.Collections.ObjectModel;
using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcousticVision.ViewModels;

public partial class AnalysisViewModel : ViewModelBase
{
    private readonly AnalysisService _analysisService;
    private readonly TestModelService _testModelService;
    private readonly RoomPreviewService _roomPreviewService;

    [ObservableProperty]
    private ObservableCollection<TestModel> _testModels = new();

    [ObservableProperty]
    private TestModel? _selectedTestModel;

    [ObservableProperty]
    private RoomPreviewViewModel? _preview;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private string _resultRoom = string.Empty;

    [ObservableProperty]
    private string _resultSource = string.Empty;

    [ObservableProperty]
    private string _resultReceiver = string.Empty;

    [ObservableProperty]
    private string _resultSourceLocation = string.Empty;

    [ObservableProperty]
    private string _resultReceiverLocation = string.Empty;

    [ObservableProperty]
    private string _resultDistance = string.Empty;

    [ObservableProperty]
    private string _resultAttenuation = string.Empty;

    [ObservableProperty]
    private string _resultDirectLevel = string.Empty;

    [ObservableProperty]
    private string _resultVolume = string.Empty;

    [ObservableProperty]
    private string _resultAbsorption = string.Empty;

    [ObservableProperty]
    private string _resultRt60 = string.Empty;

    [ObservableProperty]
    private string _resultRecommendation = string.Empty;

    [ObservableProperty]
    private string _resultFormula = string.Empty;

    public AnalysisViewModel(AnalysisService analysisService, TestModelService testModelService, RoomPreviewService roomPreviewService)
    {
        _analysisService = analysisService;
        _testModelService = testModelService;
        _roomPreviewService = roomPreviewService;
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
            var items = await _testModelService.GetAllAsync();
            TestModels = new ObservableCollection<TestModel>(items);

            if (SelectedTestModel is null && TestModels.Count > 0)
                SelectedTestModel = TestModels[0];

            await RefreshPreviewAsync();
            StatusMessage = $"Загружено тестовых моделей: {TestModels.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RunAnalysisAsync()
    {
        ClearResult();
        StatusMessage = string.Empty;

        if (SelectedTestModel is null)
        {
            StatusMessage = "Выберите тестовую модель.";
            return;
        }

        try
        {
            var result = await _analysisService.AnalyzeAsync(SelectedTestModel.Id);

            if (!result.IsSuccess)
            {
                StatusMessage = result.Message;
                return;
            }

            ResultRoom = result.RoomName;
            ResultSource = result.SourceName;
            ResultReceiver = result.ReceiverName;
            ResultSourceLocation = result.SourceLocation;
            ResultReceiverLocation = result.ReceiverLocation;
            ResultDistance = $"{result.SourceReceiverDistance:F2} м";
            ResultAttenuation = $"{result.DistanceAttenuationDb:F2} дБ";
            ResultDirectLevel = $"{result.EstimatedDirectLevelDb:F2} дБ";
            ResultVolume = $"{result.Volume:F2} м³";
            ResultAbsorption = $"{result.EquivalentAbsorptionArea:F2} м²";
            ResultRt60 = $"{result.Rt60:F3} с";
            ResultRecommendation = result.Recommendation;
            StatusMessage = result.Message;
            ResultFormula = result.FormulaName;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка анализа: {ex.Message}";
        }
    }


    partial void OnSelectedTestModelChanged(TestModel? value)
    {
        _ = RefreshPreviewAsync();
    }

    private async Task RefreshPreviewAsync()
    {
        if (SelectedTestModel is null)
        {
            Preview = new RoomPreviewViewModel
            {
                IsAvailable = false,
                Title = "Схема помещения",
                Summary = "Выберите тестовую модель для отображения схемы."
            };
            return;
        }

        try
        {
            Preview = await _roomPreviewService.BuildForTestModelAsync(SelectedTestModel.Id);
        }
        catch
        {
            Preview = new RoomPreviewViewModel
            {
                IsAvailable = false,
                Title = "Схема помещения",
                Summary = "Не удалось сформировать визуализацию помещения."
            };
        }
    }

    private void ClearResult()
    {
        ResultRoom = string.Empty;
        ResultSource = string.Empty;
        ResultReceiver = string.Empty;
        ResultSourceLocation = string.Empty;
        ResultReceiverLocation = string.Empty;
        ResultDistance = string.Empty;
        ResultAttenuation = string.Empty;
        ResultDirectLevel = string.Empty;
        ResultVolume = string.Empty;
        ResultAbsorption = string.Empty;
        ResultRt60 = string.Empty;
        ResultFormula = string.Empty; 
        ResultRecommendation = string.Empty;

    }
}