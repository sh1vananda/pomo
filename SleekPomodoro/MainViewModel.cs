using System;
using System.ComponentModel;
using System.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SleekPomodoro
{
    public enum TimerMode { Work, Break }

    public class MainViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private TimeSpan _time;
        private TimerMode _currentMode;
        private int _sessionCount;
        private TimeSpan _totalDuration;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Bound Properties
        private string _timeDisplay;
        public string TimeDisplay { get => _timeDisplay; set { _timeDisplay = value; OnPropertyChanged(nameof(TimeDisplay)); } }

        private bool _isTimerRunning;
        public bool IsTimerRunning { get => _isTimerRunning; set { _isTimerRunning = value; OnPropertyChanged(nameof(IsTimerRunning)); OnPropertyChanged(nameof(StartPauseIcon)); } }

        private double _progressValue;
        public double ProgressValue { get => _progressValue; set { _progressValue = value; OnPropertyChanged(nameof(ProgressValue)); } }

        public bool IsWorkSession => _currentMode == TimerMode.Work;
        public string StartPauseIcon => IsTimerRunning ? "\uE769" : "\uE768";
        #endregion

        public ICommand StartPauseCommand { get; }
        public ICommand ResetCommand { get; }

        public MainViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += Timer_Tick;
            StartPauseCommand = new RelayCommand(ExecuteStartPause);
            ResetCommand = new RelayCommand(ExecuteReset);
            ExecuteReset();
        }

        private void ExecuteStartPause()
        {
            if (IsTimerRunning) { _timer.Stop(); IsTimerRunning = false; }
            else { _timer.Start(); IsTimerRunning = true; }
        }

        private void ExecuteReset()
        {
            _timer.Stop();
            _currentMode = TimerMode.Work;
            _totalDuration = _time = TimeSpan.FromMinutes(25);
            IsTimerRunning = false;
            UpdateDisplay();
            OnPropertyChanged(nameof(IsWorkSession));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _time = _time.Subtract(TimeSpan.FromMilliseconds(100));
            UpdateDisplay();
            if (_time <= TimeSpan.Zero) { _time = TimeSpan.Zero; _timer.Stop(); UpdateDisplay(); SystemSounds.Asterisk.Play(); SwitchToNextMode(); }
        }

        private void SwitchToNextMode()
        {
            _currentMode = (_currentMode == TimerMode.Work) ? TimerMode.Break : TimerMode.Work;
            if (_currentMode == TimerMode.Break) { _sessionCount++; _totalDuration = _time = (_sessionCount % 4 == 0) ? TimeSpan.FromMinutes(15) : TimeSpan.FromMinutes(5); }
            else { _totalDuration = _time = TimeSpan.FromMinutes(25); }
            OnPropertyChanged(nameof(IsWorkSession));
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            TimeDisplay = _time.ToString(@"mm\:ss");
            ProgressValue = _totalDuration.TotalSeconds > 0 ? 100 * (_totalDuration.TotalSeconds - _time.TotalSeconds) / _totalDuration.TotalSeconds : 0;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public event EventHandler CanExecuteChanged { add => CommandManager.RequerySuggested += value; remove => CommandManager.RequerySuggested -= value; }
        public RelayCommand(Action execute) { _execute = execute ?? throw new ArgumentNullException(nameof(execute)); }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
    }
}