using System.Collections.ObjectModel;

namespace HeatHavnAppProject.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<ViewModelBase> Tabs { get; }
    public string[] TabNames { get; } = {
        "📁 Asset Manager", "📊 Source Data Manager", "📈 Optimizer",
        "📦 Result Data Manager", "📉 Data Visualization",
        "🧪 Scenario Selection", "📅 Time Period", "⚙️ Settings", "🧰 Advanced Tools"
    };

    public MainWindowViewModel()
    {
        Tabs = new ObservableCollection<ViewModelBase>
        {
            new AssetManagerViewModel(), // Only this one is real for now
            new ViewModelBase(), new ViewModelBase(), new ViewModelBase(),
            new ViewModelBase(), new ViewModelBase(), new ViewModelBase(),
            new ViewModelBase(), new ViewModelBase()
        };
    }
}
