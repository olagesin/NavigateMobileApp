namespace NFCProj.DTOs
{
    public class GetLocationTagDto
    {
        public string Id { get; set; }
        public GetLocationDto Location { get; set; }

        public GetTagDto Tag { get; set; }

        public string UserName { get; set; }

        public List<GetParkingRecordDto> Records { get; set; }
    }
}
