namespace NFCProj.DTOs
{
    public class ErrorItemModel
    {
        public string Key { get; set; }
        public List<string> ErrorMessages { get; set; }
    }

    public class ValidationErrorModel
    {
        public List<string> Email { get; set; }
        public List<string> Password { get; set; }
        public List<string> PhoneNumber { get; set; }
    }
}
