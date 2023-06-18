namespace NFCProj.DTOs
{
    public class GetLocationTagDto
    {
        public string Id { get; set; }
        public GetLocationDto Location { get; set; }

        public GetTagDto Tag { get; set; }

        public GetEventDto Event { get; set; }

        public string UserName { get; set; }

        public List<GetParkingRecordDto> Records { get; set; }
    }

    public class AddParkingRecordDto
    {
        public string TagSerialNumber { get; set; }
        public string EventId { get; set; }
        public CheckInType CheckInType { get; set; }
        public CheckInLocation CheckInLocation { get; set; }
    }
    public enum CheckInType
    {
        Arrival,
        Departure
    }

    public enum CheckInLocation
    {
        MainEntrance,
        ParkingStation
    }
}
