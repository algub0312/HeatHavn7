using System.Collections.ObjectModel;
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
        var assetManagerVM = new AssetManagerViewModel();
        Tabs = new ObservableCollection<object>
        {
            new ProductionUnitsViewModel(assetManagerVM),
            new ViewModelBase(),
            new ViewModelBase(),
            new ViewModelBase(),
        };

        CurrentView = Tabs.First();
    }
}
