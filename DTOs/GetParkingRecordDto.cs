namespace NFCProj.DTOs
{
    public class GetParkingRecordDto
    {
        public DateTime ArrivalTime { get; set; }
        public DateTime? CheckInTime { get; set; }

        public DateTime? DepartureTime { get; set; }
        public DateTime? CheckoutTime { get; set; }
    }
}
