namespace API.Errors
{
    public class ApiException
    {
        public ApiException(int statudCode, string message, string details)
        {
            StatudCode = statudCode;
            Message = message;
            Details = details;
        }

        public int StatudCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        
        
        
        
        
    }
}