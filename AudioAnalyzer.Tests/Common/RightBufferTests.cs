using AudioMark.Core.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AudioAnalyzer.Tests.Common
{
    public class RightBufferTests
    {
        private Tuple<int, double> RunSumTestWriteNoWait(int operationsCount, int readDelay, int writeDelay)
        {
            var ringBuffer = new RingBuffer(10, 10);
            int successCount = 0;
            double readSum = 0.0f;            
            bool done = false;

            var writeThread = new Thread(() =>
            {
                for (int i = 0; i < operationsCount; i++)
                {
                    successCount += ringBuffer.WriteNoWait((chunk) => { chunk[0] = chunk[0] == 1 ? 0 : 1; return 1; }) ? 1 : 0;
                    Thread.Sleep(writeDelay);
                }

                done = true;
            });

            var readThread = new Thread(() =>
            {
                while (!done)
                {
                    ringBuffer.Read((chunk, len) => { readSum += chunk[0]; });
                    Thread.Sleep(readDelay);
                }
            });

            writeThread.Start();
            readThread.Start();

            writeThread.Join();
            readThread.Join();

            return new Tuple<int, double>(successCount, readSum);
        }

        private Tuple<int, double> RunSumTestReadNoWait(int operationsCount, int readDelay, int writeDelay)
        {
            var ringBuffer = new RingBuffer(10, 10);
            int successCount = 0;
            double readSum = 0.0f;
            bool done = false;

            var writeThread = new Thread(() =>
            {
                while (!done) { 
                    ringBuffer.Write((chunk) => { chunk[0] = chunk[0] == 1 ? 0 : 1; return 1; });
                    Thread.Sleep(writeDelay);
                }                
            });

            var readThread = new Thread(() =>
            {
                for (int i = 0; i < operationsCount; i++)
                {
                    successCount += ringBuffer.ReadNoWait((chunk, len) => { readSum += chunk[0]; }) ? 1 : 0;
                    Thread.Sleep(readDelay);
                }

                done = true;
            });

            writeThread.Start();
            Thread.Sleep(100);

            readThread.Start();

            readThread.Join();
            writeThread.Join();            

            return new Tuple<int, double>(successCount, readSum);
        }


        [Test]
        public void ShoudAlwaysWriteWhenReadsAreFasterThenWriteNoWaits()
        {
            var operationsCount = 1000;
            var result = RunSumTestWriteNoWait(operationsCount, 1, 3);

            Assert.AreEqual(operationsCount, result.Item1);
            Assert.Less(operationsCount / 2.0f - result.Item2, float.Epsilon);
        }

        [Test]
        public void ShouldHaveFailedWritesWHenReadsAreSlowerThenWriteNoWaits()
        {
            var operationsCount = 1000;
            var result = RunSumTestWriteNoWait(operationsCount, 3, 1);

            Assert.Less(result.Item1, operationsCount);            
        }

        [Test]
        public void ShoudAlwaysReadWhenReadNoWaitsAreSlowerThenWrites()
        {
            var operationsCount = 1000;
            var result = RunSumTestReadNoWait(operationsCount, 3, 1);

            Assert.AreEqual(operationsCount, result.Item1);
            Assert.Less(operationsCount / 2.0f - result.Item2, float.Epsilon);
        }

        [Test]
        public void ShouldHaveFailedReadsWhenReadNoWaitsAreFsterThenWrites()
        {
            var operationsCount = 1000;
            var result = RunSumTestReadNoWait(operationsCount, 1, 3);

            Assert.Less(result.Item1, operationsCount);
        }
    }
}
