using AudioMark.Core.AudioData;
using AudioMark.Core.Common;
using AudioMark.Core.Fft;
using AudioMark.Core.Generators;
using AudioMark.Core.Measurements.Analysis;
using AudioMark.Core.Measurements.Settings.Common;
using AudioMark.Core.Measurements.StopConditions;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements.Common
{
    public abstract class MeasurementBase : IMeasurement
    {
      
        protected volatile bool _running;
        public bool Running
        {
            get => _running;
        }

        private TaskCompletionSource<bool> _completionSource = null;

        public string CurrentActivityDescription { get; protected set; }
        public int CurrentActivityIndex { get; protected set; }
        public int ActivitiesCount { get; protected set; }

        public abstract TimeSpan? Remaining { get; }
        
        private DateTime _startedAt;
        public TimeSpan Elapsed
        {
            get => DateTime.Now.Subtract(_startedAt).Duration();
        }

        public string Name { get; set; }
        public virtual Spectrum Result { get; protected set; }

        public IMeasurementSettings Settings { get; private set; }
        public IAnalysisResult AnalysisResult { get; protected set; }
        
        public event EventHandler<Spectrum> DataUpdate;
        public event EventHandler<bool> Complete;
        public event EventHandler<Exception> Error;

        private MeasurementBase()
        {            
        }

        public MeasurementBase(IMeasurementSettings settings) : this()
        {
            Settings = settings;
        }

        internal MeasurementBase(IMeasurementSettings settings, IAnalysisResult result) : this(settings)
        {
            AnalysisResult = result;
        }

        public async Task Run()
        {
            AnalysisResult = null;

            _running = true;
            _completionSource = new TaskCompletionSource<bool>();
            _startedAt = DateTime.Now;

            RunInternal();

            await _completionSource.Task;
        }

        protected abstract void RunInternal();

       
        public void Stop()
        {
            Stop(true);
        }

        protected void Stop(bool interrupted)
        {
            if (_running)
            {
                StopInternal(interrupted);

                _running = false;
                
                _completionSource.TrySetResult(interrupted);

                Complete?.Invoke(this, !interrupted);
            }
        }

        protected abstract void StopInternal(bool interrupted);

        protected abstract bool CheckSignalPresent(Spectrum data);
        public abstract void Update();

        protected void OnDataUpdate(Spectrum data) => DataUpdate?.Invoke(this, data);
        protected void OnComplete(bool result) => Complete?.Invoke(this, result);
        protected void OnError(Exception e) => Error?.Invoke(this, e);
    }
}
