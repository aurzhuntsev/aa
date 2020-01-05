using System;
using System.Collections.Generic;
using System.Text;

namespace PortAudioWrapper
{
    public enum PaErrorCode
    {
        PaNoError = 0,

        PaNotInitialized = -10000,
        PaUnanticipatedHostError,
        PaInvalidChannelCount,
        PaInvalidSampleRate,
        PaInvalidDevice,
        PaInvalidFlag,
        PaSampleFormatNotSupported,
        PaBadIODeviceCombination,
        PaInsufficientMemory,
        PaBufferTooBig,
        PaBufferTooSmall,
        PaNullCallback,
        PaBadStreamPtr,
        PaTimedOut,
        PaInternalError,
        PaDeviceUnavailable,
        PaIncompatibleHostApiSpecificStreamInfo,
        PaStreamIsStopped,
        PaStreamIsNotStopped,
        PaInputOverflowed,
        PaOutputUnderflowed,
        PaHostApiNotFound,
        PaInvalidHostApi,
        PaCanNotReadFromACallbackStream,
        PaCanNotWriteToACallbackStream,
        PaCanNotReadFromAnOutputOnlyStream,
        PaCanNotWriteToAnInputOnlyStream,
        PaIncompatibleStreamHostApi,
        PaBadBufferPtr
    }

    public enum PaStreamCallbackResult
    {
        PaContinue = 0,
        PaComplete = 1,
        PaAbort = 2
    }

    public enum PaHostApiTypeId
    {
        PaInDevelopment = 0,
        PaDirectSound = 1,
        PaASIO = 3,
        PaSoundManager = 4,
        PaCoreAudio = 5,
        PaOSS = 7,
        PaALSA = 8,
        PaAL = 9,
        PaBeOS = 10,
        PaWDMKS = 11,
        PaJACK = 12,
        PaWASAPI = 13,
        PaAudioScienceHPI = 14
    }

    public enum PaSampleFormat
    {
        PaFloat32 = 0x00000001,
        PaInt32 = 0x00000002,
        PaInt24 = 0x00000004,
        PaInt16 = 0x00000008
    }
}
