using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcousticVision.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    public MainWindowViewModel()
    {
        CurrentViewModel = new OverviewViewModel();
    }

    [RelayCommand]
    private void ShowOverview() => CurrentViewModel = new OverviewViewModel();

    [RelayCommand]
    private async void ShowRooms()
    {
        var vm = App.Services!.GetRequiredService<RoomsViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowRoomSurfaces()
    {
        var vm = App.Services!.GetRequiredService<RoomSurfacesViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowMaterials()
    {
        var vm = App.Services!.GetRequiredService<MaterialsViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowTextures()
    {
        var vm = App.Services!.GetRequiredService<TexturesViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowSources()
    {
        var vm = App.Services!.GetRequiredService<SourcesViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowReceivers()
    {
        var vm = App.Services!.GetRequiredService<ReceiversViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowTestModels()
    {
        var vm = App.Services!.GetRequiredService<TestModelsViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }

    [RelayCommand]
    private async void ShowAnalysis()
    {
        var vm = App.Services!.GetRequiredService<AnalysisViewModel>();
        await vm.InitializeAsync();
        CurrentViewModel = vm;
    }
}
