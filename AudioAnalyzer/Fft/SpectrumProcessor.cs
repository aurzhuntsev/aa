using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Fft
{
    /* TODO: Implement error handling */
    public class SpectrumProcessor
    {
       
        internal class ProcessingItem
        {
            public double[] Data { get; set; }
            public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
        }

        public Spectrum Data { get; set; }

        public event EventHandler<Spectrum> OnItemProcessed;

        private readonly int MaxTasks = Math.Max(1, Environment.ProcessorCount - 1);

        public int WindowSize { get; private set; }
        public double OverlapFactor { get; private set; }
        public int MaxFrequency { get; set; }
        public WindowFunctions WindowFunction { get; set; }

        private int CorrectedWindowSize => WindowSize % 2 == 0 ? WindowSize + 2 : WindowSize + 1;

        private RingBuffer _buffer = null;

        private double[] _accumulator = null;
        private int _accumulatorCounter = 0;
        private object _sync = new object();

        private double[] _windowFunction = null;
        private static readonly List<(WindowFunctions, int, double[])> _windowFunctionsCache = new List<(WindowFunctions, int, double[])>();

        private List<ProcessingItem> _processingItems;
        

        public SpectrumProcessor(int windowSize, double overlapFactor, int maxFrequency, WindowFunctions windowFunction)
        {
            WindowSize = windowSize;
            OverlapFactor = overlapFactor;
            MaxFrequency = maxFrequency;
            WindowFunction = windowFunction;

            Data = new Spectrum(WindowSize, MaxFrequency);
            _windowFunction = ConstructWindowFunction();

            Reset();
        }

        /* TODO: Implement properly */
        public void Reset()
        {
            lock (_sync)
            {
                if (OverlapFactor != 0.0)
                {
                    _buffer = new RingBuffer((int)Math.Ceiling(1.0 / OverlapFactor) + 1,
                                 (int)Math.Ceiling(CorrectedWindowSize * OverlapFactor));
                }
                else
                {
                    _buffer = new RingBuffer(1, CorrectedWindowSize);
                }

                _accumulator = new double[_buffer.ChunkLength];
                _accumulatorCounter = 0;

                _processingItems = new List<ProcessingItem>();
                
                Data.Reset();
            }
        }

        public void Add(double value)
        {
            lock (_sync)
            {
                AddInternal(value);
            }
        }

        private double[] ConstructWindowFunction()
        {
            var cacheItem = _windowFunctionsCache.FirstOrDefault(c => c.Item1 == WindowFunction && c.Item2 == CorrectedWindowSize);
            if (cacheItem.Item3 != null)
            {
                return cacheItem.Item3;
            }
            else
            {
                double[] result;
                if (WindowFunction == WindowFunctions.FlatTop)
                {
                    result = WindowsHelper.FlatTop(CorrectedWindowSize);
                }
                else if (WindowFunction == WindowFunctions.Hann)
                {
                    result = WindowsHelper.Hann(CorrectedWindowSize);                    
                }
                else if (WindowFunction == WindowFunctions.Taylor)
                {
                    result = WindowsHelper.Taylor(CorrectedWindowSize, WindowsHelper.DefaultTaylorBars, WindowsHelper.DefaultTaylorSLL);
                }
                else
                {
                    result = WindowsHelper.Rectangular(CorrectedWindowSize);
                }

                _windowFunctionsCache.Add((WindowFunction, CorrectedWindowSize, result));
                return result;
            }
        }

        private void AddInternal(double value)
        {
            _accumulator[_accumulatorCounter] = value;
            _accumulatorCounter++;

            if (_accumulatorCounter == _accumulator.Length)
            {
                _accumulatorCounter = 0;

                _buffer.Write((data) =>
                {
                    _accumulator.CopyTo(data, 0);
                    return _accumulator.Length;
                });

                if (_buffer.Count == _buffer.Length)
                {
                    _buffer.Read((data, length) => { });
                }

                if (_buffer.Count == _buffer.Length - 1)
                {
                    ProcessingItem currentItem = null;

                    foreach (var item in _processingItems)
                    {
                        if (item.Semaphore.CurrentCount > 0)
                        {
                            currentItem = item;
                            break;
                        }
                    }

                    if (currentItem == null)
                    {
                        if (_processingItems.Count < MaxTasks)
                        {
                            currentItem = new ProcessingItem() { Data = new double[CorrectedWindowSize] };
                            _processingItems.Add(currentItem);
                        }
                        else
                        {
                            currentItem = _processingItems[0];
                        }
                    }

                    currentItem.Semaphore.Wait();

                    var j = 0;
                    for (var i = 0; i < _buffer.Length - 1; i++)
                    {
                        _buffer.Peek(i, (data, length) =>
                        {
                            for (var k = 0; k < length; k++)
                            {
                                currentItem.Data[j] = data[k];
                                j++;
                                if (j == currentItem.Data.Length)
                                {
                                    break;
                                }
                            }
                        });
                    }

                    Task.Run(() => ProcessItem(currentItem));
                }
            }
        }

        private void ProcessItem(ProcessingItem processingItem)
        {
            try
            {                
                for (var i = 0; i < CorrectedWindowSize; i++)
                {
                    processingItem.Data[i] *= _windowFunction[i];
                }

                MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(processingItem.Data, AppSettings.Current.Fft.WindowSize,
                   MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

                var inverseSize = 2.0 / AppSettings.Current.Fft.WindowSize;
                for (var i = 0; i < processingItem.Data.Length; i++)
                {
                    processingItem.Data[i] = Math.Abs(processingItem.Data[i] * inverseSize);
                }

                Data.Set(processingItem.Data);
                OnItemProcessed?.DynamicInvoke(this, Data);
            }
            finally
            {
                processingItem.Semaphore.Release();
            }
        }
    }
}
