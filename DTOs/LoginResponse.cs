namespace NFCProj.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RoleName { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
