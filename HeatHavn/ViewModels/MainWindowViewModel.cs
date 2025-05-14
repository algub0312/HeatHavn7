using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using HeatHavnAppProject.ViewModels;
using ReactiveUI;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel; // Required for Dispatcher

namespace HeatHavnAppProject.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<object> Tabs { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        private bool _isPaneOpen;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => this.RaiseAndSetIfChanged(ref _isPaneOpen, value);
        }
        private ICommand _togglePaneCommand;
        public ICommand TogglePaneCommand => _togglePaneCommand ??= new RelayCommand(() =>
        {
            IsPaneOpen = !IsPaneOpen;
        });
        public MainWindowViewModel()
        {
            try
            {
                // Initialize the IsPaneOpen property
                _isPaneOpen = true;

                var sourceManager = new SourceDataManagerViewModel();
                sourceManager.LoadSummerDataHeat("Data/summerperiod.csv");
                sourceManager.LoadWinterDataHeat("Data/winterperiod.csv");

                sourceManager.LoadSummerDataEl("Data/summerperiod.csv");
                sourceManager.LoadWinterDataEl("Data/winterperiod.csv");

                var assetManagerVM = new AssetManagerViewModel();
                var assetManager = new AssetManager();

                Tabs = new ObservableCollection<object>
                {
                    new HomePageViewModel(),
                    new ProductionUnitsViewModel(assetManagerVM),
                    new HeatDemandViewModel(sourceManager),
                    new ElectricityPricesViewModel(sourceManager),
                    new Optimizer(sourceManager, assetManager, assetManagerVM),
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
}