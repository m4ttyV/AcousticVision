using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcousticVision.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public MainWindowViewModel()
    {
        // стартовый экран
        CurrentViewModel = new OverviewViewModel();
    }

    [RelayCommand]
    private void ShowOverview() => CurrentViewModel = new OverviewViewModel();

    [RelayCommand]
    private void ShowRooms() => CurrentViewModel = new RoomsViewModel();

    [RelayCommand]
    private void ShowMaterials() => CurrentViewModel = new MaterialsViewModel();

    [RelayCommand]
    private void ShowSources() => CurrentViewModel = new SourcesViewModel();

    [RelayCommand]
    private void ShowReceivers() => CurrentViewModel = new ReceiversViewModel();

    [RelayCommand]
    private void ShowTestModels() => CurrentViewModel = new TestModelsViewModel();

    [RelayCommand]
    private void ShowAnalysis() => CurrentViewModel = new AnalysisViewModel();
}