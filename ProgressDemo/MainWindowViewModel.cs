using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProgressDemo;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public int? Min
    {
        get;
        set => SetField(ref field, value);
    }
    
    public int? Max
    {
        get;
        set => SetField(ref field, value);
    }
    
    public int Value
    {
        get;
        set => SetField(ref field, value);
    }
    
    public ICommand StartCommand { get; }

    public MainWindowViewModel()
    {
        StartCommand = new LambdaCommand(async _ => 
        {
            var min = Min ?? 0; 
            var max = Max ?? 100; 
            
            var progress = new Progress<int>();
            progress.ProgressChanged += (_, i) => Value = i;
            await StartAsync(min, max, progress);
        });
    }
    
    private async Task StartAsync(int min, int max, IProgress<int> progress)
    {
        for (int i = min; i <= max; i++)
        {
            await Task.Delay(1000);
            progress.Report(i);
        }
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}