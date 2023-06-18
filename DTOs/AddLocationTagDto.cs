using System.ComponentModel.DataAnnotations;

namespace NFCProj.DTOs
{
    public class AddLocationTagDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string SerialNumber { get; set; }

        [Required]
        public string SourceLocationId { get; set; }

        [Required]
        public string DestinationLocationId { get; set; }

        [Required]
        public string EventId { get; set; }
    }
}
