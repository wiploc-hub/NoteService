namespace NoteServiceApi.Models
{
    public class MethodResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";

        public void SetToException(Exception exception)
        {
            Success = false;
            Message = exception.Message;
        }

        public void SetFailedIfSubFailed(MethodResponse subResponse)
        {
            if (subResponse.Success == false)
            {
                Success = false;
                Message = subResponse.Message;
            }
        }
    }

    public class MethodResponse<T> : MethodResponse
    {
        public T? Result { get; set; }
    }

}
