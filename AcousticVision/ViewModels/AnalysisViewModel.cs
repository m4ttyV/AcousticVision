using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using AcousticVision.Models;
using AcousticVision.Services;
using AcousticVision.Views;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

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

    [ObservableProperty]
    private string _resultAverageAbsorption = string.Empty;

    [ObservableProperty]
    private string _resultArticulation = string.Empty;

    [ObservableProperty]
    private string _resultRt60Factor = string.Empty;

    [ObservableProperty]
    private string _resultPerceivedClarity = string.Empty;

    public bool HasResult => !string.IsNullOrWhiteSpace(ResultRoom);

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
            TestModels = new ObservableCollection<TestModel>(items.OrderBy(x => x.Id));

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
            ResultAverageAbsorption = $"{result.AverageAbsorption:F3}";
            ResultRt60 = $"{result.Rt60:F3} с";
            ResultFormula = result.FormulaName;
            ResultArticulation = $"{result.SourceArticleFactor:F2}";
            ResultRt60Factor = $"{result.Rt60Factor:F2}";
            ResultPerceivedClarity = $"{result.PerceivedClarity * 100.0:F0}% ({result.PerceivedClarityLevel})";
            ResultRecommendation = result.Recommendation;
            StatusMessage = result.Message;
            OnPropertyChanged(nameof(HasResult));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка анализа: {ex.Message}";
        }
    }

    [RelayCommand]
    private void OpenReport()
    {
        if (!HasResult)
        {
            StatusMessage = "Сначала выполните расчёт, чтобы открыть отчёт.";
            return;
        }

        var reportWindow = new AnalysisReportWindow
        {
            DataContext = this
        };

        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
            reportWindow.Show(desktop.MainWindow);
        else
            reportWindow.Show();
    }

    [RelayCommand]
    private void PrintReport()
    {
        if (!HasResult)
        {
            StatusMessage = "Сначала выполните расчёт, чтобы подготовить отчёт к печати.";
            return;
        }

        try
        {
            var reportDirectory = Path.Combine(Path.GetTempPath(), "AcousticVision");
            Directory.CreateDirectory(reportDirectory);

            var reportFileName = $"analysis-report-{DateTime.Now:yyyyMMdd-HHmmss}.html";
            var reportPath = Path.Combine(reportDirectory, reportFileName);

            File.WriteAllText(reportPath, BuildPrintableHtml(), Encoding.UTF8);

            Process.Start(new ProcessStartInfo
            {
                FileName = reportPath,
                UseShellExecute = true
            });

            StatusMessage = "Печатная версия отчёта открыта в браузере. Если окно печати не появилось автоматически, используйте Ctrl+P.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Не удалось подготовить отчёт к печати: {ex.Message}";
        }
    }

    partial void OnSelectedTestModelChanged(TestModel? value)
    {
        ClearResult();
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
        ResultAverageAbsorption = string.Empty;
        ResultRt60 = string.Empty;
        ResultFormula = string.Empty;
        ResultArticulation = string.Empty;
        ResultRt60Factor = string.Empty;
        ResultPerceivedClarity = string.Empty;
        ResultRecommendation = string.Empty;
        OnPropertyChanged(nameof(HasResult));
    }

    private string BuildPrintableHtml()
    {
        var generatedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("ru-RU"));
        var selectedModel = SelectedTestModel?.DisplayName ?? "—";

        return $$"""
               <!doctype html>
               <html lang="ru">
               <head>
                   <meta charset="utf-8">
                   <title>Отчёт AcousticVision</title>
                   <style>
                       :root {
                           color-scheme: light;
                           --text: #111827;
                           --muted: #4B5563;
                           --border: #D0D7DE;
                           --card: #FFFFFF;
                           --page: #F5F7FA;
                           --accent: #2563EB;
                       }

                       * {
                           box-sizing: border-box;
                       }

                       body {
                           margin: 0;
                           padding: 28px;
                           color: var(--text);
                           background: var(--page);
                           font-family: Arial, Helvetica, sans-serif;
                       }

                       .report {
                           max-width: 1080px;
                           margin: 0 auto;
                       }

                       .header,
                       .card {
                           background: var(--card);
                           border: 1px solid var(--border);
                           border-radius: 14px;
                           padding: 18px;
                           margin-bottom: 16px;
                       }

                       h1 {
                           margin: 0 0 8px;
                           font-size: 26px;
                       }

                       h2 {
                           margin: 0 0 14px;
                           font-size: 18px;
                       }

                       .muted {
                           color: var(--muted);
                           line-height: 1.45;
                       }

                       .schema {
                           display: flex;
                           gap: 24px;
                           align-items: flex-start;
                           flex-wrap: wrap;
                       }

                       table {
                           width: 100%;
                           border-collapse: collapse;
                           font-size: 14px;
                       }

                       td {
                           border-bottom: 1px solid #E5E7EB;
                           padding: 9px 8px;
                           vertical-align: top;
                       }

                       td:first-child {
                           width: 290px;
                           font-weight: 700;
                       }

                       tr:last-child td {
                           border-bottom: none;
                       }

                       .surface-grid {
                           display: grid;
                           grid-template-columns: repeat(2, minmax(240px, 1fr));
                           gap: 8px 18px;
                           margin-top: 14px;
                           font-size: 13px;
                       }

                       .surface-item {
                           display: flex;
                           gap: 8px;
                           align-items: center;
                       }

                       .swatch {
                           width: 14px;
                           height: 14px;
                           border: 1px solid var(--border);
                           border-radius: 4px;
                           flex: 0 0 auto;
                       }

                       @media print {
                           body {
                               padding: 0;
                               background: white;
                           }

                           .header,
                           .card {
                               break-inside: avoid;
                           }
                       }
                   </style>
                   <script>
                       window.addEventListener('load', function () {
                           setTimeout(function () { window.print(); }, 450);
                       });
                   </script>
               </head>
               <body>
                   <main class="report">
                       <section class="header">
                           <h1>Отчёт AcousticVision</h1>
                           <div class="muted">Тестовая модель: {{H(selectedModel)}}<br>Дата формирования: {{H(generatedAt)}}</div>
                       </section>

                       <section class="card">
                           <h2>Схема помещения с компасом</h2>
                           <div class="schema">
                               {{BuildRoomSvg()}}
                           </div>
                           {{BuildSurfaceLegendHtml()}}
                       </section>

                       <section class="card">
                           <h2>Результаты расчёта</h2>
                           {{BuildResultTableHtml()}}
                       </section>
                   </main>
               </body>
               </html>
               """;
    }

    private string BuildResultTableHtml()
    {
        var rows = new[]
        {
            ("Помещение", ResultRoom),
            ("Источник", ResultSource),
            ("Координаты источника", ResultSourceLocation),
            ("Приёмник", ResultReceiver),
            ("Координаты приёмника", ResultReceiverLocation),
            ("Расстояние источник–приёмник", ResultDistance),
            ("Использованная формула", ResultFormula),
            ("Ослабление прямого сигнала", ResultAttenuation),
            ("Условный уровень в точке", ResultDirectLevel),
            ("Артикуляция источника", ResultArticulation),
            ("Коэффициент RT60", ResultRt60Factor),
            ("Оценочная разборчивость", ResultPerceivedClarity),
            ("Объём помещения", ResultVolume),
            ("Эквивалентное поглощение", ResultAbsorption),
            ("Средний коэффициент поглощения", ResultAverageAbsorption),
            ("RT60", ResultRt60),
            ("Рекомендация", ResultRecommendation)
        };

        var builder = new StringBuilder("<table>");
        foreach (var (label, value) in rows)
        {
            builder.Append("<tr><td>")
                .Append(H(label))
                .Append("</td><td>")
                .Append(H(value))
                .Append("</td></tr>");
        }

        builder.Append("</table>");
        return builder.ToString();
    }

    private string BuildRoomSvg()
    {
        if (Preview is null || !Preview.IsAvailable)
            return $"<p class=\"muted\">{H(Preview?.Summary ?? "Схема помещения недоступна.")}</p>";

        var p = Preview;
        var canvasWidth = p.CanvasWidth;
        var canvasHeight = p.CanvasHeight;
        var compassX = canvasWidth + 28;
        const double compassY = 22;
        const double compassSize = 86;
        var svgWidth = canvasWidth + compassSize + 56;
        var svgHeight = Math.Max(canvasHeight, compassY + compassSize + 8);
        var sourceCenterX = p.SourceX + p.MarkerSize / 2;
        var sourceCenterY = p.SourceY + p.MarkerSize / 2;
        var receiverCenterX = p.ReceiverX + p.MarkerSize / 2;
        var receiverCenterY = p.ReceiverY + p.MarkerSize / 2;

        return $$"""
               <svg xmlns="http://www.w3.org/2000/svg" width="{{F(svgWidth)}}" height="{{F(svgHeight)}}" viewBox="0 0 {{F(svgWidth)}} {{F(svgHeight)}}" role="img" aria-label="Схема помещения">
                   <rect x="0" y="0" width="{{F(canvasWidth)}}" height="{{F(canvasHeight)}}" rx="12" fill="#F9FAFB" stroke="#D0D7DE"/>
                   <rect x="{{F(p.RoomX)}}" y="{{F(p.RoomY)}}" width="{{F(p.RoomWidth)}}" height="{{F(p.RoomHeight)}}" fill="#FFFFFF" stroke="#D0D7DE"/>
                   <rect x="{{F(p.RoomX)}}" y="{{F(p.RoomY)}}" width="{{F(p.RoomWidth)}}" height="{{F(p.WallThickness)}}" fill="{{BrushToHex(p.NorthBrush)}}"/>
                   <rect x="{{F(p.RoomX)}}" y="{{F(p.SouthWallY)}}" width="{{F(p.RoomWidth)}}" height="{{F(p.WallThickness)}}" fill="{{BrushToHex(p.SouthBrush)}}"/>
                   <rect x="{{F(p.RoomX)}}" y="{{F(p.RoomY)}}" width="{{F(p.WallThickness)}}" height="{{F(p.RoomHeight)}}" fill="{{BrushToHex(p.WestBrush)}}"/>
                   <rect x="{{F(p.EastWallX)}}" y="{{F(p.RoomY)}}" width="{{F(p.WallThickness)}}" height="{{F(p.RoomHeight)}}" fill="{{BrushToHex(p.EastBrush)}}"/>

                   <circle cx="{{F(sourceCenterX)}}" cy="{{F(sourceCenterY)}}" r="{{F(p.MarkerSize / 2)}}" fill="#EF4444"/>
                   <text x="{{F(sourceCenterX)}}" y="{{F(sourceCenterY + 3.5)}}" text-anchor="middle" font-family="Arial" font-size="10" font-weight="700" fill="#FFFFFF">S</text>
                   <circle cx="{{F(receiverCenterX)}}" cy="{{F(receiverCenterY)}}" r="{{F(p.MarkerSize / 2)}}" fill="#10B981"/>
                   <text x="{{F(receiverCenterX)}}" y="{{F(receiverCenterY + 3.5)}}" text-anchor="middle" font-family="Arial" font-size="10" font-weight="700" fill="#FFFFFF">R</text>

                   <rect x="{{F(compassX)}}" y="{{F(compassY)}}" width="{{F(compassSize)}}" height="{{F(compassSize)}}" rx="12" fill="#FFFFFF" stroke="#D0D7DE"/>
                   <text x="{{F(compassX + compassSize / 2)}}" y="{{F(compassY + 18)}}" text-anchor="middle" font-family="Arial" font-size="13" font-weight="700" fill="#4B5563">С</text>
                   <text x="{{F(compassX + compassSize / 2)}}" y="{{F(compassY + compassSize - 9)}}" text-anchor="middle" font-family="Arial" font-size="13" font-weight="700" fill="#4B5563">Ю</text>
                   <text x="{{F(compassX + 14)}}" y="{{F(compassY + compassSize / 2 + 5)}}" text-anchor="middle" font-family="Arial" font-size="13" font-weight="700" fill="#4B5563">З</text>
                   <text x="{{F(compassX + compassSize - 14)}}" y="{{F(compassY + compassSize / 2 + 5)}}" text-anchor="middle" font-family="Arial" font-size="13" font-weight="700" fill="#4B5563">В</text>
                   <line x1="{{F(compassX + compassSize / 2)}}" y1="{{F(compassY + 28)}}" x2="{{F(compassX + compassSize / 2)}}" y2="{{F(compassY + compassSize - 28)}}" stroke="#4B5563" stroke-width="2"/>
                   <line x1="{{F(compassX + 28)}}" y1="{{F(compassY + compassSize / 2)}}" x2="{{F(compassX + compassSize - 28)}}" y2="{{F(compassY + compassSize / 2)}}" stroke="#4B5563" stroke-width="2"/>
                   <polygon points="{{F(compassX + compassSize / 2)}},{{F(compassY + 28)}} {{F(compassX + compassSize / 2 - 6)}},{{F(compassY + 40)}} {{F(compassX + compassSize / 2 + 6)}},{{F(compassY + 40)}}" fill="#2563EB"/>
               </svg>
               """;
    }

    private string BuildSurfaceLegendHtml()
    {
        if (Preview is null || !Preview.IsAvailable)
            return string.Empty;

        var p = Preview;
        var rows = new[]
        {
            (p.NorthBrush, p.NorthLabel),
            (p.SouthBrush, p.SouthLabel),
            (p.EastBrush, p.EastLabel),
            (p.WestBrush, p.WestLabel),
            (p.FloorBrush, p.FloorLabel),
            (p.CeilingBrush, p.CeilingLabel)
        };

        var builder = new StringBuilder("<div class=\"surface-grid\">");
        foreach (var (brush, label) in rows)
        {
            builder.Append("<div class=\"surface-item\"><span class=\"swatch\" style=\"background:")
                .Append(BrushToHex(brush))
                .Append("\"></span><span>")
                .Append(H(label))
                .Append("</span></div>");
        }

        builder.Append("</div>");
        return builder.ToString();
    }

    private static string BrushToHex(IBrush? brush)
    {
        if (brush is ISolidColorBrush solidBrush)
            return $"#{solidBrush.Color.R:X2}{solidBrush.Color.G:X2}{solidBrush.Color.B:X2}";

        return "#E5E7EB";
    }

    private static string H(string? value)
    {
        return WebUtility.HtmlEncode(value ?? "—");
    }

    private static string F(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}