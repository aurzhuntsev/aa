using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioMark.Core.Common
{
    public class RingBuffer
    {        
        class Item
        {
            public double[] Data { get; set; }
            public int Length { get; set; }

            public Item(int length)
            {
                Data = new double[length];
            }
        }

        public int Length { get; private set; }
        public int ChunkLength { get; private set; }

        private Item[] buffer = null;        

        private volatile int head = 0;
        private volatile int tail = 0;
        private volatile int count = 0;

        public int Count => count;

        private readonly object sync = new object();

        public RingBuffer(int length, int chunkLength)
        {
            Length = length;
            ChunkLength = chunkLength;

            buffer = new Item[Length];

            for (var i = 0; i < length; i++)
            {
                buffer[i] = new Item(chunkLength);
            }
        }

        public bool WriteNoWait(Func<double[], int> writeAction)
        {
            if (Monitor.IsEntered(sync))
            {
                return false;
            }

            Monitor.Enter(sync);
            {
                if (count > 0 && head == tail)
                {
                    Monitor.Exit(sync);
                    return false;
                }

                buffer[head].Length = writeAction(buffer[head].Data);                

                IncrementHead();
            }
            Monitor.Exit(sync);

            return true;
        }

        public bool Write(Func<double[], int> writeAction)
        {
            if (count > 0 && head == tail)
            {
                return false;
            }

            Monitor.Enter(sync);
            {
                if (count > 0 && head == tail)
                {
                    Monitor.Exit(sync);
                    return false;
                }

                buffer[head].Length = writeAction(buffer[head].Data);
                IncrementHead();
            }
            Monitor.Exit(sync);

            return true;

        }

        public bool ReadNoWait(Action<double[], int> readAction)
        {
            if (count == 0)
            {
                return false;
            }

            if (Monitor.IsEntered(sync))
            {
                return false;
            }
            Monitor.Enter(sync);
            {
                readAction(buffer[tail].Data, buffer[tail].Length);
                IncrementTail();
            }
            Monitor.Exit(sync);

            return true;
        }

        public bool Read(Action<double[], int> readAction)
        {
            if (count == 0)
            {
                return false;
            }

            Monitor.Enter(sync);
            {
                readAction(buffer[tail].Data, buffer[tail].Length);
                IncrementTail();
            }
            Monitor.Exit(sync);

            return true;
        }       

        public bool Peek(int index, Action<double[], int> peekAction)
        {
            if (index > Length - 1)
            {
                throw new IndexOutOfRangeException();
            }

            if (count == 0)
            {
                return false;
            }

            var i = tail + index;
            if (i >= Length)
            {
                i = i - Length;
            }

            Monitor.Enter(sync);
            {
                peekAction(buffer[i].Data, buffer[i].Length);                
            }
            Monitor.Exit(sync);

            return true;
        }

        private void IncrementHead()
        {
            head++;
            if (head == Length)
            {
                head = 0;
            }

            count++;            
        }

        private void IncrementTail()
        {
            tail++;
            if (tail == Length)
            {
                tail = 0;
            }

            count--;            
        }
    }
}
