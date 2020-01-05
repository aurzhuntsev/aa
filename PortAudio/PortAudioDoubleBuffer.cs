using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PortAudioWrapper
{
    public abstract class PortAudioDoubleBuffer : SafeHandle
    {
        internal IntPtr Pointer => handle;
        public int Length { get; internal set; }

        public override bool IsInvalid => handle == IntPtr.Zero;

        internal unsafe PortAudioDoubleBuffer(IntPtr pointer) : base(IntPtr.Zero, true)
        {
            SetHandle(pointer);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }

        public double this[int index]
        {
            get
            {
                return Read(index);
            }
            set
            {
                Write(value, index);
            }
        }

        public abstract double Read(int index);
        public abstract void Write(double value, int index);
    }

    internal class PortAudioDoubleFloatBuffer : PortAudioDoubleBuffer
    {
        private unsafe float* _buffer;

        public unsafe PortAudioDoubleFloatBuffer(IntPtr pointer) : base(pointer)
        {
            _buffer = (float*)pointer;
        }

        public override unsafe double Read(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            return (double)_buffer[index];
        }

        public override unsafe void Write(double value, int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            _buffer[index] = (float)value;
        }
    }


    internal class PortAudioDoubleInt24Buffer : PortAudioDoubleBuffer
    {
        private unsafe byte* _buffer;

        public unsafe PortAudioDoubleInt24Buffer(IntPtr pointer) : base(pointer)
        {
            _buffer = (byte*)pointer;
        }

        public override unsafe double Read(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            var offset = index * 3;
            float val = 0;

            val = (float)(_buffer[offset + 2] | (_buffer[offset + 1] << 8) | (_buffer[offset + 2] << 16));
            return val / (double)(1 << 23);
        }

        public override unsafe void Write(double value, int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            var offset = index * 3;
            var denormalizedValue = (int)(value * (1 << 23));
            
            _buffer[offset + 0] = (byte)(denormalizedValue & 0x000000ff);
            _buffer[offset + 1] = (byte)((denormalizedValue >> 8) & 0x000000ff);
            _buffer[offset + 2] = (byte)((denormalizedValue >> 16) & 0x000000ff);            
        }
    }
}
