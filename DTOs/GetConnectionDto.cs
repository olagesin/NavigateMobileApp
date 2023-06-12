namespace NFCProj.DTOs
{
    public class GetConnectionDto
    {
        public string Id { get; set; }
        public GetLocationDto SourceLocation { get; set; }
        public GetLocationDto DestinationLocation { get; set; }
        public double DistanceInKm { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        //public double AverageSpeed { get; set; }
    }
}
