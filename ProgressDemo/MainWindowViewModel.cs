using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProgressDemo;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private CancellationTokenSource? _cts;
    
    private bool _isPaused;
    private bool _isStarted;
    private bool _isStopped;
    
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
    public ICommand StopCommand { get; }
    public ICommand PauseCommand { get; }

    public MainWindowViewModel()
    {
        _isStarted = false;
        _isPaused = false;
        _isStopped = false;
        
        StartCommand = new LambdaCommand(
            async _ => 
        {
            if (_isStarted) return;
            _cts ??= new CancellationTokenSource();
            
            var min = Min ?? 0; 
            var max = Max ?? 100;

            if (_isPaused)
            {
                min = Value;
            }
            
            if (_isPaused || _isStopped)
            {
                _cts = new CancellationTokenSource();
                
                _isPaused = false;
                _isStopped = false;
            }

            _isStarted = true;
            
            var progress = new Progress<int>();
            progress.ProgressChanged += (_, i) => Value = i;
            await StartAsync(min, max, progress, _cts.Token);
        },
            _ => !_isStarted);
        
        StopCommand = new LambdaCommand(
            async _ =>
        {
            _isStopped = true;
            await StopAsync();
        },
            _ => _isStarted);
        PauseCommand = new LambdaCommand(async _ =>
        {
            await _cts?.CancelAsync()!;
            _isPaused = true;
            _isStarted = false;
        },
            _ => _isStarted && !_isPaused);
    }

    public async Task StopAsync()
    {
        await _cts?.CancelAsync()!;
        _isStarted = false;
        _isPaused = false;
    }
    
    private async Task StartAsync(int min, int max, IProgress<int> progress, CancellationToken token)
    {
        try 
        {
            for (int i = min; i <= max; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        
                        await Task.Delay(1000, token);
                        progress.Report(i);
                    }
        }
        catch (OperationCanceledException) {
            MessageBox.Show("Task was canceled");
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