using System.Collections.ObjectModel;
using System.Linq;
using HeatHavnAppProject.ViewModels;
using ReactiveUI;

namespace HeatHavnAppProject.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ViewModelBase> Tabs { get; } = new()
    {
        new AssetManagerViewModel(),
        new SourceDataManagerViewModel(),
        new ViewModelBase(),
        new ViewModelBase(),
        new ViewModelBase(),

    };

    private ViewModelBase _currentView;
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }


    public MainWindowViewModel()
    {
        CurrentView = Tabs.First(); // default selected tab
    }
}
