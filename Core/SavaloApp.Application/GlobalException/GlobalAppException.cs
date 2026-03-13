namespace SavaloApp.Application.GlobalException;

public class GlobalAppException : Exception
{
    public string ErrorCode { get; }

    public GlobalAppException(string errorCode) : base(errorCode)
    {
        ErrorCode = errorCode;
    }

    public GlobalAppException(string errorCode, Exception innerException) : base(errorCode, innerException)
    {
        ErrorCode = errorCode;
    }
}