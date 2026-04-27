using AcousticVision.Common;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AcousticVision.ViewModels;

public partial class SurfacePreviewItem : ObservableObject
{
    public SurfacePreviewItem(string position)
    {
        Position = position;
    }

    public string Position { get; }

    public string DisplayName => Position.ToDisplayName();

    [ObservableProperty]
    private string _materialName = "Не задано";

    [ObservableProperty]
    private double _absorption;

    [ObservableProperty]
    private IBrush _brush = new SolidColorBrush(Colors.LightGray);

    [ObservableProperty]
    private bool _isSelected;

    public string Description => $"{DisplayName}: {MaterialName} (α = {Absorption:F2})";

    public Thickness BorderThickness => IsSelected ? new Thickness(3) : new Thickness(1);

    public IBrush BorderBrush =>
        IsSelected
            ? new SolidColorBrush(Color.Parse("#F59E0B"))
            : new SolidColorBrush(Color.Parse("#64748B"));

    partial void OnAbsorptionChanged(double value)
    {
        Brush = AbsorptionColorHelper.GetBrush(value);
        OnPropertyChanged(nameof(Description));
    }

    partial void OnMaterialNameChanged(string value)
    {
        OnPropertyChanged(nameof(Description));
    }

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BorderThickness));
        OnPropertyChanged(nameof(BorderBrush));
    }
}