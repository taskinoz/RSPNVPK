namespace LzhamWrapper.Compression
{
    public enum CompressStatus
    {
        NotFinished,
        NeedsMoreInput,
        HasMoreOutput,
        FirstSuccessOrFailureCode,
        Success = FirstSuccessOrFailureCode,
        Failure,
        Failed = Failure,
        FailedInitializing,
        InvalidParameter,
        OutputBufferTooSmall,
        Force = -1
    }
}