using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HeatHavnAppProject.ViewModels;
using HeatHavnAppProject.Views;

namespace HeatHavnAppProject
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? data)
        {
            return data switch
            {
                AssetManagerViewModel => new AssetManagerView(),
                _ => throw new Exception("View not found for " + data?.GetType().Name)
            };
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
