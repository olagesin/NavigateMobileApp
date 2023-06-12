using System.ComponentModel.DataAnnotations;

namespace NFCProj.DTOs
{
    public class AddLocationTagDto
    {
        [Required]
        public string LocationId { get; set; }

        [Required]
        public string TagId { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}
