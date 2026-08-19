using System.Diagnostics;

namespace ConversionManager.Utils;

public class ConsoleProgressBar : IDisposable
{
    private readonly ulong _totalTicks;
    private readonly int _barLength;
    private readonly Stopwatch _stopwatch;
    private readonly Func<double, string> _speedFormatter;
    private bool _disposed;
    private bool _finished;
    private string _lastGeneratedOutput = "";
    
    public double CurrentProgressPercentage { get; private set; }

    public ConsoleProgressBar(
        ulong totalTicks,
        int barLength = 15,
        Func<double, string>? speedFormatter = null)
    {
        _totalTicks = totalTicks;
        _barLength = barLength;

        _speedFormatter = speedFormatter ?? (speed => speed switch
        {
            >= 1_000_000_000 => $"{speed / 1_000_000_000:F1} GB/s",
            >= 1_000_000 => $"{speed / 1_000_000:F1} MB/s",
            >= 1_000 => $"{speed / 1_000:F1} KB/s",
            _ => $"{speed:N0} B/s"
        });

        _stopwatch = new Stopwatch();
    }
    
    public string GetProgressString(ulong currentTick, string customMessage = "")
    {
        if (!_stopwatch.IsRunning && !_finished && currentTick > 0)
            _stopwatch.Start();

        double progress = _totalTicks == 0 ? 1.0 : (double)currentTick / _totalTicks;
        CurrentProgressPercentage = progress * 100;

        int progressChars = (int)Math.Round(progress * _barLength);
        progressChars = Math.Clamp(progressChars, 0, _barLength);

        double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        double speed = elapsedSeconds > 0 ? currentTick / elapsedSeconds : 0;

        string etaStr = "Calc...";
        if (progress > 0 && currentTick < _totalTicks)
        {
            double remainingSeconds = (elapsedSeconds / progress) - elapsedSeconds;
            TimeSpan remainingTime = TimeSpan.FromSeconds(remainingSeconds);

            etaStr = remainingTime.TotalHours >= 1
                ? remainingTime.ToString(@"hh\:mm\:ss")
                : remainingTime.ToString(@"mm\:ss");
        }
        else if (currentTick >= _totalTicks)
        {
            etaStr = "00:00";
            FinishBar();
        }

        string bar = new string('█', progressChars) + new string('░', _barLength - progressChars);
        string speedStr = _speedFormatter(speed);
        
        _lastGeneratedOutput = $"[{bar}] {progress:P1} | {speedStr} | ETA: {etaStr} | {customMessage}";
        return _lastGeneratedOutput;
    }
    
    public string GetLastString()
    {
        if (string.IsNullOrEmpty(_lastGeneratedOutput))
        {
            string emptyBar = new string('░', _barLength);
            return $"[{emptyBar}] 0.0% | 0 B/s | ETA: --:--";
        }
        return _lastGeneratedOutput;
    }

    public void ForceComplete()
    {
        FinishBar();
        int progressChars = _barLength;
        string bar = new string('█', progressChars);
        _lastGeneratedOutput = $"[{bar}] 100.0% | Done | ETA: 00:00";
    }

    private void FinishBar()
    {
        if (!_finished)
        {
            _finished = true;
            _stopwatch.Stop();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            FinishBar();
        }
    }
}