using ReactiveUI;
using System.Reactive;
using HeatHavnAppProject.Views;

namespace HeatHavnAppProject.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase? _currentView;
        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public ReactiveCommand<Unit, Unit> ShowAssetManagerCommand { get; }

        public MainWindowViewModel()
        {
            ShowAssetManagerCommand = ReactiveCommand.Create(() =>
            {
                CurrentView = new AssetManagerViewModel();
            });

            CurrentView = new AssetManagerViewModel();
        }
    }
}
