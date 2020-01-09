using AudioMark.Core.Common;
using AudioMark.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Measurements
{
    public class SpectralDataProcessor
    {
        public delegate void ItemProcessedEventHandler(double[] item);

        internal class ProcessingItem
        {
            public double[] Data { get; set; }
            public SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
        }

        public ItemProcessedEventHandler OnItemProcessed { get; set; }

        private readonly int MaxTasks = Environment.ProcessorCount - 1;

        public int WindowSize { get; private set; }
        public  double OverlapFactor { get; private set; }

        private int CorrectedWindowSize => WindowSize % 2 == 0 ? WindowSize + 2 : WindowSize + 1;

        private readonly RingBuffer buffer = null;

        private double[] accumulator = null;
        private int accumulatorCounter = 0;

        private List<ProcessingItem> processingItems = new List<ProcessingItem>();

        public SpectralDataProcessor(int windowSize, double overlapFactor)
        {
            WindowSize = windowSize;
            OverlapFactor = overlapFactor;
            
            buffer = new RingBuffer((int)Math.Ceiling(1.0 / OverlapFactor) + 1,
                                    (int)Math.Ceiling(CorrectedWindowSize * OverlapFactor));

            accumulator = new double[(int)Math.Ceiling(CorrectedWindowSize * OverlapFactor)];
        }

        public void Add(double value)
        {
            accumulator[accumulatorCounter] = value;
            accumulatorCounter++;

            if (accumulatorCounter == accumulator.Length)
            {
                buffer.Write((data) =>
                {
                    accumulator.CopyTo(data, 0);
                    return accumulator.Length;
                });

                if (buffer.Count == buffer.Length)
                {
                    buffer.Read((data, length) => { });
                }

                if (buffer.Count == buffer.Length - 1)
                {
                    ProcessingItem currentItem = null;

                    foreach (var item in processingItems)
                    {
                        if (item.Semaphore.CurrentCount > 0)
                        {
                            currentItem = item;
                            break;
                        }
                    }

                    if (currentItem == null)
                    {
                        if (processingItems.Count < MaxTasks)
                        {
                            currentItem = new ProcessingItem() { Data = new double[CorrectedWindowSize] };
                            processingItems.Add(currentItem);
                        }
                        else
                        {
                            currentItem = processingItems[0];
                        }
                    }

                    currentItem.Semaphore.Wait();

                    var j = 0;
                    for (var i = 0; i < buffer.Length - 1; i++)
                    {                        
                        buffer.Peek(i, (data, length) =>
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

                    (new Thread(() =>
                    {
                        try
                        {                            
                            MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(currentItem.Data, currentItem.Data.Length - 2, 
                                MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);

                            for (var i = 0; i < AppSettings.Current.Fft.WindowSize; i++)
                            {
                                currentItem.Data[i] = Math.Abs(currentItem.Data[i] / AppSettings.Current.Fft.WindowSize);
                            }

                            currentItem.Data[0] = 0.0;
                            currentItem.Data[1] = 0.0;

                            OnItemProcessed(currentItem.Data);
                        }
                        finally
                        {
                            currentItem.Semaphore.Release();
                        }
                    })).Start();
                }

                accumulatorCounter = 0;
            }
        }
    }
}
