using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using HeatHavnAppProject.Models;

namespace HeatHavnAppProject.ViewModels
{
    public class SourceDataManagerViewModel : ViewModelBase
    {
        public override string Title => "📊 Source Data Manager";
        public ObservableCollection<SourceData> SourceDataList { get; }

        public SourceDataManagerViewModel()
        {
            SourceDataList = new ObservableCollection<SourceData>();

            // Citește fișierul CSV
            var lines = File.ReadAllLines("sourcedata.csv").Skip(1); // presupunem că prima linie e header

            foreach (var line in lines)
            {
                var parts = line.Split(',');

                if (parts.Length >= 5)
                {
                    SourceDataList.Add(new SourceData
                    {
                        Season = parts[0],
                        TimeFrom = parts[1],
                        TimeTo = parts[2],
                        HeatDemand = double.TryParse(parts[3], out var h) ? h : 0,
                        ElectricityPrice = double.TryParse(parts[4], out var e) ? e : 0
                    });
                }
            }
        }
    }
}
