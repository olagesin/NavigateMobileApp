using System.ComponentModel.DataAnnotations;

namespace NFCProj.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage ="please input a valid email address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage ="please input a password")]
        public string Password { get; set; }
    }
}
