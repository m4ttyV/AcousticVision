using AcousticVision.Models;
using AcousticVision.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AcousticVision.ViewModels;

public partial class MaterialsViewModel : ViewModelBase
{
    private readonly MaterialService _materialService;
    private List<Material> _allMaterials = new();

    [ObservableProperty]
    private ObservableCollection<Material> _materials = new();

    [ObservableProperty]
    private Material? _selectedMaterial;

    [ObservableProperty]
    private string _newMaterialName = string.Empty;

    [ObservableProperty]
    private string _newNoiseCancelation = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _editMaterialName = string.Empty;

    [ObservableProperty]
    private string _editNoiseCancelation = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public MaterialsViewModel(MaterialService materialService)
    {
        _materialService = materialService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        _allMaterials = (await _materialService.GetAllAsync())
            .OrderBy(x => x.Id)
            .ToList();
        ApplyFilter();
        StatusMessage = $"Загружено материалов: {Materials.Count}";
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task AddMaterialAsync()
    {
        StatusMessage = string.Empty;

        var name = NewMaterialName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название материала.";
            return;
        }

        if (!TryParseFactor(NewNoiseCancelation, out var value))
        {
            StatusMessage = "Введите корректное числовое значение коэффициента в диапазоне от 0 до 1.";
            return;
        }

        try
        {
            await _materialService.AddAsync(name, value);
            NewMaterialName = string.Empty;
            NewNoiseCancelation = string.Empty;
            await LoadAsync();
            StatusMessage = "Материал успешно добавлен.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при добавлении: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task UpdateSelectedAsync()
    {
        if (SelectedMaterial is null)
        {
            StatusMessage = "Выберите материал для изменения.";
            return;
        }

        var name = EditMaterialName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            StatusMessage = "Введите название материала для изменения.";
            return;
        }

        if (!TryParseFactor(EditNoiseCancelation, out var value))
        {
            StatusMessage = "Введите корректное числовое значение коэффициента в диапазоне от 0 до 1.";
            return;
        }

        try
        {
            var selectedId = SelectedMaterial.Id;
            await _materialService.UpdateAsync(selectedId, name, value);
            await LoadAsync();
            SelectedMaterial = Materials.FirstOrDefault(x => x.Id == selectedId);
            StatusMessage = "Материал успешно изменён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при изменении: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedMaterial = null;
        EditMaterialName = string.Empty;
        EditNoiseCancelation = string.Empty;
        StatusMessage = "Выбор материала сброшен.";
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedMaterial is null)
        {
            StatusMessage = "Выберите материал для удаления.";
            return;
        }

        try
        {
            await _materialService.DeleteAsync(SelectedMaterial.Id);
            SelectedMaterial = null;
            EditMaterialName = string.Empty;
            EditNoiseCancelation = string.Empty;
            await LoadAsync();
            StatusMessage = "Материал удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка при удалении: {ex.Message}";
        }
    }

    partial void OnSelectedMaterialChanged(Material? value)
    {
        if (value is null)
        {
            EditMaterialName = string.Empty;
            EditNoiseCancelation = string.Empty;
            return;
        }

        EditMaterialName = value.Name;
        EditNoiseCancelation = value.NoiseCancelation.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<Material> filtered = _allMaterials;
        var query = SearchText?.Trim();

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(x =>
                x.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                x.NoiseCancelation.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    .Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        Materials = new ObservableCollection<Material>(filtered);

        if (SelectedMaterial is not null)
        {
            SelectedMaterial = Materials.FirstOrDefault(x => x.Id == SelectedMaterial.Id);
        }
    }

    private static bool TryParseFactor(string? input, out double value)
    {
        var normalized = (input ?? string.Empty).Replace(',', '.');
        if (double.TryParse(normalized, System.Globalization.CultureInfo.InvariantCulture, out value))
        {
            return value >= 0 && value <= 1;
        }

        value = 0;
        return false;
    }
}
