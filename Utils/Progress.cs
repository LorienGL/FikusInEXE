using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FikusIn.Utils
{
    public class Progress: ObservableObjectBase, IDisposable
    {
        private static readonly ConcurrentDictionary<Guid, Progress> _progressList = []; // This will be used in many threads
        private static readonly BindingList<Progress> _bindedProgressList = []; // Thid list is used by the main thread only (UI)

        public double Minimum { get; }
        public double Maximum { get; }
        public bool IsDeterminate { get; }

        private Guid Id { get; }

        private double _current = 0;
        public double Current 
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        private static DispatcherTimer? refreshBindedListTimer = null;

        // Multi thred supported
        public Progress(double min, double max, bool isDeterminate)
        {
            Minimum = min;
            Maximum = max; 
            IsDeterminate = isDeterminate;
            Id = Guid.NewGuid();

            while(!_progressList.TryAdd(Id, this)) // Should never be false, ever
                Id = Guid.NewGuid();
        }

        // This is to create a copy of each progress to be used by main thread
        private Progress(double min, double max, bool isDeterminate, double current, Guid id)
        {
            Minimum = min;
            Maximum = max;
            IsDeterminate = isDeterminate;
            Current = current;
            Id = id;
        }

        // Only called once at program start
        public static BindingList<Progress> StartProgressList()
        {
            // Start a timer (150ms) that will go on til program ends
            refreshBindedListTimer = new DispatcherTimer();
            refreshBindedListTimer.Interval = TimeSpan.FromMilliseconds(50);
            refreshBindedListTimer.Tick += refreshBindedListTimer_Tick;
            refreshBindedListTimer.Start();

            return _bindedProgressList; 
        }

        private static void refreshBindedListTimer_Tick(object? sender, EventArgs e)
        {
            var deletedList = _bindedProgressList.Where(p => !_progressList.ContainsKey(p.Id)).ToList();
            foreach (var progress in deletedList)
                _bindedProgressList.Remove(progress);

            var newList = _progressList.Values.Where(p => !_bindedProgressList.Any(p2 => p2.Id == p.Id)).ToList();
            foreach (var progress in newList)
                _bindedProgressList.Add(new Progress(progress.Minimum, progress.Maximum, progress.IsDeterminate, progress.Current, progress.Id));

            foreach(var progress in _bindedProgressList)
            {
                Progress thProgress;
                if (_progressList.TryGetValue(progress.Id, out thProgress))
                    progress.Current = thProgress.Current;
            }
        }

        // Must dispose the object so the progress bar is released too
        public void Dispose() => _progressList.Remove(Id, out _);
    }
}
