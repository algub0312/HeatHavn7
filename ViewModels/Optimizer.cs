using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace HeatHavnAppProject.ViewModels;

public class Optimizer : ViewModelBase
{
    public override string Title => "📈 Optimizer";
    private DateTimeOffset? _startDate = DateTimeOffset.Now;
    public DateTimeOffset? StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }


     private DateTimeOffset? _endDate = DateTimeOffset.Now.AddDays(1);
    public DateTimeOffset? EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }


    private bool _scenario1Enabled;
    public bool Scenario1Enabled
    {
        get => _scenario1Enabled;
        set => this.RaiseAndSetIfChanged(ref _scenario1Enabled, value);
    }

    public ReactiveCommand<Unit, Unit> OptimizeCommand { get; }

    public Optimizer()
    {
        OptimizeCommand = ReactiveCommand.Create(() =>
        {
            // Placeholder logic
            Console.WriteLine("Optimize clicked!");
            Console.WriteLine($"From {StartDate} to {EndDate} | Scenario 1: {Scenario1Enabled}");
        });
    }
}
