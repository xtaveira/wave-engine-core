namespace Microwave.Domain.DTOs
{
    public class OperationResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public string? ErrorCode { get; private set; }

        private OperationResult(bool isSuccess, string message, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            ErrorCode = errorCode;
        }

        public static OperationResult CreateSuccess(string message = "")
        {
            return new OperationResult(true, message);
        }

        public static OperationResult CreateError(string message, string? errorCode = null)
        {
            return new OperationResult(false, message, errorCode);
        }
    }

    public class OperationResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public string? ErrorCode { get; private set; }
        public T? Data { get; private set; }

        private OperationResult(bool isSuccess, string message, T? data = default, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            ErrorCode = errorCode;
        }

        public static OperationResult<T> CreateSuccess(T data, string message = "")
        {
            return new OperationResult<T>(true, message, data);
        }

        public static OperationResult<T> CreateError(string message, string? errorCode = null)
        {
            return new OperationResult<T>(false, message, default, errorCode);
        }
    }
}