using System.Collections.ObjectModel;
using HeatHavnAppProject.Models;

public class AssetManagerViewModel
{
    private readonly AssetManager _assetManager;

    public ObservableCollection<ProductionUnit> AllUnits { get; }

    public AssetManagerViewModel()
    {
        _assetManager = new AssetManager();
        AllUnits = new ObservableCollection<ProductionUnit>(_assetManager.GetAllUnits());
    }
}
