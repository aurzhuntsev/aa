using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PortAudioWrapper
{
    class PortAudioException : Exception
    {
        public PaErrorCode PaError { get; private set; }

        public PortAudioException(int paError)
        {
            PaError = (PaErrorCode)paError;
        }

        public PortAudioException(PaErrorCode paError)
        {
            PaError = paError;
        }

        public override string Message
        {
            get
            {
                return Marshal.PtrToStringAnsi(Imports.Pa_GetErrorText((int)PaError));
            }
        }
    }
}
