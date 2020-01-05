using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using PortAudioWrapper;
using System.Threading;
using System.Linq;
using System.Text;
using MathNet.Numerics.Statistics;
using System.IO;

using AudioAnalyzer.UI.SpectralView;
using AudioAnalyzer.Measurements;
using Avalonia.Threading;
using MathNet.Numerics.Distributions;

namespace AudioAnalyzer.UI
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public WriteableBitmap Bitmap { get; set; } =
        new WriteableBitmap(
   new PixelSize(
   (int)1024,
   (int)768),
   new Vector(96, 96),
   Avalonia.Platform.PixelFormat.Bgra8888);

        EventWaitHandle h = new EventWaitHandle(false, EventResetMode.ManualReset);
        bool updated = false;
        double[] tx = null;

        DateTime? start = null;

        private void InitializeComponent()
        {

            AvaloniaXamlLoader.Load(this);

            DataContext = this;

            PortAudio.Initialize();
            var r = new Random();
            double[] noise = null;
            int lastSeconds = 0;
            var m = new SpectrumMeasurement()
            {
                OnDataUpdate = (data) =>
                {
                    if (tx == null)
                    {
                        tx = new double[data.Data.Length];
                        start = DateTime.Now;
                    }

                    bool done = true;
                    int failed = 0;

                    double avg = 0;

                    if ((int)DateTime.Now.Subtract(start.Value).TotalSeconds % 10 == 0 && lastSeconds != (int)DateTime.Now.Subtract(start.Value).TotalSeconds)
                    {

                        for (var i = 0; i < data.Data.Length; i++)
                        {
                            if (data.Count < 2)
                            {
                                continue;
                            }

                            lastSeconds = (int)DateTime.Now.Subtract(start.Value).TotalSeconds;
                            var v = data.GetMeanAndStandardDeviation(i);

                            double t = 1.645 * (v.StandardDeviation / Math.Sqrt(data.Count));
                            tx[i] = t;

                            if (v.Mean / t < 20.0)
                            {
                                failed++;
                            }                            
                        }

                        updated = true;
                        Trace.WriteLine(failed);
                        if (failed == 0)
                        {
                            int qwr = 1231;
                        }
                    }



                    /*if (DateTime.Now.Subtract(start.Value).TotalSeconds > 60)
                    {
                        using (var sw = new StreamWriter("noise.dat"))
                        {
                            foreach (var t in tx)
                            {
                                sw.WriteLine(t);
                            }
                        }

                        int a = 10;
                    }*/


                }
            };

            m.Run();
        }

        private float[] TestSignal()
        {
            var avg = new List<float>();
            var vr = new List<float>();
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);

            var istreamParameters = new PaStreamParameters()
            {
                ChannelCount = 2,
                Device = PortAudio.Instance.GetDefaultInputDeviceIndex(),
                SampleFormat = PaSampleFormat.PaFloat32,
                SuggestedLatency = 0.1
            };


            var ostreamParameters = new PaStreamParameters()
            {
                ChannelCount = 2,
                Device = PortAudio.Instance.GetDefaultOutputDeviceIndex(),
                SampleFormat = PaSampleFormat.PaFloat32,
                SuggestedLatency = 0.1
            };


            var table = new float[96000];
            var rec = new List<double>();
            var x = 0;
            for (x = 0; x < 96; x++)
            {
                table[x] = (float)(0.99426 * Math.Sin(x * (2.0 * Math.PI / 96.0)));
            }

            x = 0;
            Process.GetCurrentProcess().PriorityBoostEnabled = true;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;

            int runs = 0;
            handle.Reset();
            DateTime now = DateTime.Now;

            using (var stream = new PortAudioStream(istreamParameters, ostreamParameters, 96000, 0, 0,
                (i, o, time, status) =>
                {

                    for (var c = 0; c < o.Length; c += 2)
                    {
                        var v = table[x];
                        o[c] = v;
                        o[c + 1] = v;

                        x++;
                        if (x == 96)
                        {
                            x = 0;
                        }
                    }

                    if (runs > 10)
                    {
                        for (var c = 0; c < i.Length; c += 2)
                        {
                            rec.Add(i[c + 1]);
                        }
                    }

                    if (DateTime.Now.Subtract(now).TotalSeconds > 10)
                    {
                        handle.Set();
                        return PaStreamCallbackResult.PaComplete;
                    }

                    runs++;
                    return PaStreamCallbackResult.PaContinue;
                }))
            {
                stream.Start();
            }

            handle.WaitOne();

            using (var sw = new StreamWriter("sound.dat"))
            {
                sw.WriteLine(rec.Count);

                foreach (var p in rec)
                {
                    sw.WriteLine(p);
                }
            }

            return new float[0];
        }

        private unsafe void Test(object sender, PointerPressedEventArgs e)
        {
            var cnv = sender as Grid;
            var image = (cnv.Children[0] as Image);



            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler((o, e) =>
            {
                if (updated)
                {
                    updated = false;
                    using (var buf = Bitmap.Lock())
                    {
                        var view = new SpectralView.SpectralView()
                        {
                            Width = Bitmap.PixelSize.Width,
                            Height = Bitmap.PixelSize.Height,
                            Data = tx
                        };

                        view.Render(buf.Address);
                    }

                    image.Source = Bitmap;
                    image.InvalidateVisual();
                }
            });
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 40);
            dispatcherTimer.Start();

            return;
            //TestSignal();
            /*
            float[] arr = null;
            using (var sr = new StreamReader("sound.dat"))
            {
                var len = int.Parse(sr.ReadLine());
                arr = new float[len];

                for (var i = 0; i < len; i++)
                {
                    arr[i] = float.Parse(sr.ReadLine());
                }
            }

            int cnt = 0;
            var c = new double[48000];
            var f = new List<float>();
            var m = new List<double>();
            var std = new List<double>();
            List<double>[] t = new List<double>[48000];
            for (var i = 0; i < t.Length; i++)
            {
                t[i] = new List<double>();
            }

            do
            {
                var w = arr.Skip(48000 * cnt).Take(48002).Select(_ => (double)_).ToArray();
                if (w.Length < 48002)
                {
                    break;
                }

                MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal(w, w.Length - 2, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);
                var tw = w.Take(48000).Select(_ => _ / 48000).ToArray();                
                for (var i = 0; i < tw.Length; i++)
                {
                    t[i].Add(Math.Abs(tw[i]));
                }

                cnt++;
            } while (true);


            var tx = new double[48000];
            for (var i = 0; i < t.Length; i++)
            {
                tx[i] = t[i].Average();
            }

            var mx = tx.Max();
            for (var i = 0; i < tx.Length; i++)
            {
                tx[i] = (tx[i] / mx);
            }

            var cnv = sender as Grid;
            var image = (cnv.Children[0] as Image);
            var width = Bitmap.PixelSize.Width;

            var x0 = 0;

            var cw = Math.Log10(t.Length) / Bitmap.Size.Width;
            var ch = Math.Log10(1.0) / Bitmap.Size.Height;

            var val = 0.0;

            var max = tx.Max();
            var min = tx.Where(qq => qq > 0.0).Min();
            var div = tx[1000] / tx[2000];

            using (var buf = Bitmap.Lock())
            {
                var view = new SpectralView.SpectralView()
                {
                    Width = Bitmap.PixelSize.Width,
                    Height = Bitmap.PixelSize.Height,
                    Data = tx.ToArray()
                };

                view.Render(buf.Address);
            }

            image.Source = Bitmap;
            image.InvalidateVisual();

            var sb = new StringBuilder();
            var v = 5.0;
            var r = new Random();

            for (var i = 0; i < 1000; i++)
            {
                var sgn = r.NextDouble() > 0.5 ? 1.0 : -1.0;
                var n = r.NextDouble();

                sb.AppendFormat("{0}\n", v + sgn * n);
            }

            var xxx = sb.ToString();
            */
        }

        private unsafe void SetPixel(IntPtr target, int height, int width, int x, int y, uint color)
        {
            var ptr = (uint*)target;
            ptr += (uint)(width * y + x);

            *ptr = color;
        }
    }
}