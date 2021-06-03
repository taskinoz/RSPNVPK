namespace LzhamWrapper.Decompression
{
    public enum DecompressStatus
    {
        NotFinished,
        HasMoreOutput,
        NeedsMoreInput,
        FirstSuccessOrFailureCode = Success,
        Success = 3,
        Failure,
        FailedInitializing = Failure,
        DestinationBufferTooSmall,
        ExpectedMoreRawBytes,
        BadCode,
        Adler32,
        BadRawBlock,
        BadCompBlockSyncCheck,
        BadZlibHeader,
        NeedSeedBytes,
        BadSeedBytes,
        BadSyncBlock,
        InvalidParameter
    }
}