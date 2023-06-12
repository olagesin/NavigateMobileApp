using System.ComponentModel.DataAnnotations;

namespace NFCProj.DTOs
{
    public class AddLocationDto
    {
        [Required]
        public string LocationType { get; set; }

        [Required]
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        [Required]
        public string Name { get; set; }


        public int SpaceCount { get; set; }
    }

    public class AddConnectionDto
    {
        [Required]
        public string SourceLocationId { get; set; }

        [Required]
        public string DestinationLocationId { get; set; }
        public double DistanceInKm { get; set; }
        public double AverageSpeed { get; set; }
    }
}
