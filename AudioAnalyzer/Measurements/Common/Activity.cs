using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using AudioMark.Core.Common;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.StopConditions;

namespace AudioMark.Core.Measurements.Common
{
    public class Activity<TSink>
    {        
        private List<IStopCondition> _stopConditions = new List<IStopCondition>();
        IEnumerable<IStopCondition> StopConditions
        {
            get => _stopConditions;
        }
        
        public Dictionary<int, IGenerator> Generators { get; } = new Dictionary<int, IGenerator>();
        public Dictionary<int, IDataSink<TSink>> DataSinks { get; } = new Dictionary<int, IDataSink<TSink>>();

        public event EventHandler OnComplete;
        public event EventHandler<Exception> OnError;

        public TimeSpan? Remaining
        {
            get
            {
                if (!StopConditions.Any() || StopConditions.Any(stopCondition => !stopCondition.Remaining.HasValue))
                {
                    return null;
                }

                return StopConditions.Min(stopCondition => stopCondition.Remaining.Value);
            }
        }

        public string Description { get; }

        public Activity(string description)
        {
            Description = description;            
        }
        
        public void CheckStopConditions()
        {
            foreach (var stopCondition in StopConditions)
            {
                stopCondition.Check();
            }
        }

        public void RegisterStopCondition(IStopCondition stopCondition)
        {
            stopCondition.OnMet += (sender, e) =>
            {
                OnComplete?.Invoke(this, null);
            };

            stopCondition.OnError += (sender, innerException) =>
            {
                var exception = new Exception("StopCondition evaluation failed", innerException);
                OnError?.Invoke(this, exception);
            };

            _stopConditions.Add(stopCondition);
        }

        public void AddGenerator(int channel, IGenerator generator)
        {
            if (!Generators.ContainsKey(channel - 1))
            {
                Generators.Add(channel - 1, generator);
            }
            else
            {
                throw new InvalidOperationException($"Generator for channel {channel} is already registered.");
            }
        }

        public void AddSink(int channel, IDataSink<TSink> sink)
        {
            if (!DataSinks.ContainsKey(channel - 1))
            {
                DataSinks.Add(channel - 1, sink);
            }
            else
            {
                throw new InvalidOperationException($"Sink for channel {channel} is already registered.");
            }
        }

        public IDataSink<TSink> GetSink(int channel) => DataSinks[channel - 1];
            
        public void SetStopConditions()
        {
            foreach (var stopCondition in _stopConditions)
            {
                stopCondition.Set();
            }
        }
    }
}
