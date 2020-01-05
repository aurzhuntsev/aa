using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PortAudioWrapper
{
    public class UnmanagedStruct<T> : SafeHandle where T : struct
    {
        public override bool IsInvalid => handle == IntPtr.Zero;

        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException();
                }

                return Marshal.PtrToStructure<T>(handle);
            }
            set
            {
                Marshal.StructureToPtr<T>(value, handle, HasValue);
                HasValue = true;
            }
        }

        public bool HasValue { get; private set; }

        public IntPtr Pointer
        {
            get
            {
                return HasValue ? handle : IntPtr.Zero;
            }
        }

        public UnmanagedStruct() : base(IntPtr.Zero, true)
        {
            IntPtr handle = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            SetHandle(handle);
        }

        public UnmanagedStruct(T value) : this()
        {
            Value = value;
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                Marshal.FreeHGlobal(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
