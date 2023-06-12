namespace NFCProj.DTOs
{
    public class GlobalResponse<T>
    {
        public T Data { get; set; }
        public List<ErrorItemModel> Errors { get; set; }

        public GlobalResponse()
        {
            Errors = new List<ErrorItemModel>();
        }
    }

    public class GlobalResponseForValidationErrors<T>
    {
        public T Data { get; set; }

        public ValidationErrorModel Errors { get; set; }
        public int Status { get; set; }
    }
}
