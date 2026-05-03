using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AcousticVision.Views;

public partial class AnalysisReportWindow : Window
{
    public AnalysisReportWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}