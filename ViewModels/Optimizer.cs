using System;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace HeatHavnAppProject.ViewModels;

public class Optimizer : ViewModelBase
{
    public override string Title => "📈 Optimizer";
    // Zile disponibile
public List<int> Days { get; } = Enumerable.Range(1, 31).ToList();
// Ore disponibile
public List<int> Hours { get; } = Enumerable.Range(0, 24).ToList();
// Luni disponibile
public List<string> Months { get; } = new() { "March", "August" };

// START DATE
private int _startDay = 1;
public int StartDay
{
    get => _startDay;
    set
    {
        this.RaiseAndSetIfChanged(ref _startDay, value);
        UpdateStartDate();
    }
}

private int _startHour = 0;
public int StartHour
{
    get => _startHour;
    set
    {
        this.RaiseAndSetIfChanged(ref _startHour, value);
        UpdateStartDate();
    }
}

private string _startMonth = "March";
public string StartMonth
{
    get => _startMonth;
    set
    {
        this.RaiseAndSetIfChanged(ref _startMonth, value);
        UpdateStartDate();
    }
}

private DateTimeOffset? _startDate = DateTimeOffset.Now;
public DateTimeOffset? StartDate
{
    get => _startDate;
    private set => this.RaiseAndSetIfChanged(ref _startDate, value);
}

private void UpdateStartDate()
{
    var month = StartMonth == "August" ? 8 : 3;
    StartDate = new DateTimeOffset(new DateTime(2024, month, StartDay, StartHour, 0, 0));
}

private int _endDay = 1;
public int EndDay
{
    get => _endDay;
    set
    {
        this.RaiseAndSetIfChanged(ref _endDay, value);
        UpdateEndDate();
    }
}

private int _endHour = 0;
public int EndHour
{
    get => _endHour;
    set
    {
        this.RaiseAndSetIfChanged(ref _endHour, value);
        UpdateEndDate();
    }
}

private string _endMonth = "March";
public string EndMonth
{
    get => _endMonth;
    set
    {
        this.RaiseAndSetIfChanged(ref _endMonth, value);
        UpdateEndDate();
    }
}

private DateTimeOffset? _endDate = DateTimeOffset.Now;
public DateTimeOffset? EndDate
{
    get => _endDate;
    private set => this.RaiseAndSetIfChanged(ref _endDate, value);
}

private void UpdateEndDate()
{
    var month = EndMonth == "August" ? 8 : 3;
    EndDate = new DateTimeOffset(new DateTime(2024, month, EndDay, EndHour, 0, 0));
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
            Debug.WriteLine("Optimize clicked!");
            Debug.WriteLine($"From {StartDate} to {EndDate} | Scenario 1: {Scenario1Enabled}");
        });
    }
}