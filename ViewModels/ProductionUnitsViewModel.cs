using System.Collections.ObjectModel;
using HeatHavnAppProject.Models;
namespace HeatHavnAppProject.ViewModels;
public class ProductionUnitsViewModel : ViewModelBase
{
    public override string Title => "📁 Production Units";
    public ObservableCollection<ProductionUnit> Units { get; }

    public ProductionUnitsViewModel(AssetManagerViewModel assetManagerViewModel)
    {
        Units = assetManagerViewModel.AllUnits;
    }
}
