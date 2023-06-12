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
    }

    //public class GetLocationTagDto
    //{
    //    public string Id { get; set; }
    //    public GetLocationDto Location { get; set; }

    //    public GetTagDto Tag { get; set; }

    //    public string UserName { get; set; }

    //    public List<GetParkingRecordDto> Records { get; set; }
    //}
}
