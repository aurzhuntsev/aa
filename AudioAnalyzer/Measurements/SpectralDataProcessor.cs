﻿using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    /* TODO: Implement error handling */
    public class SpectralDataProcessor : IDataSink<SpectralData>
    {
        internal class ProcessingItem
        {
            public double[] Data { get; set; }
            public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
        }

        public bool Silent { get; set; }
        public SpectralData Data { get; set; }

        public event EventHandler<SpectralData> OnItemProcessed;

        private readonly int MaxTasks = Math.Max(1, Environment.ProcessorCount - 1);

        public int WindowSize { get; private set; }
        public double OverlapFactor { get; private set; }
        public int MaxFrequency { get; set; }

        private int CorrectedWindowSize => WindowSize % 2 == 0 ? WindowSize + 2 : WindowSize + 1;
        
        private RingBuffer _buffer = null;

        private double[] _accumulator = null;
        private int _accumulatorCounter = 0;

        private List<ProcessingItem> _processingItems;

        public SpectralDataProcessor(int windowSize, double overlapFactor, int maxFrequency)
        {
            WindowSize = windowSize;
            OverlapFactor = overlapFactor;
            MaxFrequency = maxFrequency;

            Reset();
        }

        /* TODO: Implement properly */
        public void Reset()
        {
            _buffer = new RingBuffer((int)Math.Ceiling(1.0 / OverlapFactor) + 1,
                         (int)Math.Ceiling(CorrectedWindowSize * OverlapFactor));

            _accumulator = new double[(int)Math.Ceiling(CorrectedWindowSize * OverlapFactor)];
            _accumulatorCounter = 0;

            _processingItems = new List<ProcessingItem>();

            Data = new SpectralData(WindowSize, MaxFrequency);
        }

        public void Add(double value)
        {
            _accumulator[_accumulatorCounter] = value;
            _accumulatorCounter++;

            if (_accumulatorCounter == _accumulator.Length)
            {
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

                    ThreadPool.QueueUserWorkItem((item) =>
                    {
                        try
                        {
                            var processingItem = (ProcessingItem)item;
                            MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(currentItem.Data, AppSettings.Current.Fft.WindowSize,
                               MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

                            for (var i = 0; i < AppSettings.Current.Fft.WindowSize; i++)
                            {
                                processingItem.Data[i] = Math.Abs(processingItem.Data[i] / AppSettings.Current.Fft.WindowSize);
                            }

                            processingItem.Data[0] = 0.0;
                            processingItem.Data[1] = 0.0;

                            Data.Set(processingItem.Data);

                            if (!Silent)
                            {
                                OnItemProcessed?.Invoke(this, Data);
                            }
                        }
                        finally
                        {
                            currentItem.Semaphore.Release();
                        }
                    }, currentItem);
                }
                _accumulatorCounter = 0;
            }
        }
    }
}
