namespace HMS_360_PMS.HMS_360_PMS.API.Models
{
    //public class ApiResponse<T>
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //    public T Data { get; set; }

    //    public static ApiResponse<T> SuccessResponse(T data, string message = "")
    //        => new ApiResponse<T> { Success = true, Message = message, Data = data };

    //    public static ApiResponse<T> Fail(string message)
    //        => new ApiResponse<T> { Success = false, Message = message };
    //}

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> SuccessResult(T data, string message = null)
            => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Failure(string message)
            => new() { Success = false, Data = default, Message = message };
    }
}
