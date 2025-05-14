using HeatHavnAppProject.Models;
using ReactiveUI;

public class SelectableProductionUnit : ReactiveObject
{
    public ProductionUnit Unit { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public SelectableProductionUnit(ProductionUnit unit)
    {
        Unit = unit;
        IsSelected = false;
    }
}
