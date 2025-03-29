using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using HeatHavnAppProject.ViewModels;
using ReactiveUI;
namespace HeatHavnAppProject.ViewModels;
public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<object> Tabs { get; }

    private object _currentView;
    public object CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }


     public MainWindowViewModel()
{
    try
    {
        var sourceManager = new SourceDataManagerViewModel();
        sourceManager.LoadSummerData("Data/summerperiod.csv");
        sourceManager.LoadWinterData("Data/winterperiod.csv");

        var assetManagerVM = new AssetManagerViewModel();

        Tabs = new ObservableCollection<object>
        {
            new ProductionUnitsViewModel(assetManagerVM),
            new HeatDemandViewModel(sourceManager),
            new ViewModelBase()
        };

        CurrentView = Tabs.First();
    }
    catch (Exception ex)
    {
        Debug.WriteLine("💥 EROARE: " + ex.Message);
    }
}


}
