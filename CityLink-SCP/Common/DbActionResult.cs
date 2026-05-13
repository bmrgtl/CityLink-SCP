namespace CityLink_SCP.Common
{
    public class DbActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public DbActionResult() { }
        public DbActionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
	}
    public class DbActionResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public T? Data { get; set; }

        public DbActionResult() { }
        public DbActionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
        public DbActionResult(bool success, string message, T? data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static implicit operator DbActionResult<T>(DbActionResult result)
        {
            return new DbActionResult<T>
            {
                Success = result.Success,
                Message = result.Message
            };
        }
    }
}
