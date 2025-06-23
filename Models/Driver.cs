namespace ZaiEats.Models
{
    public class Driver
    {
        public int DriverId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string VehicleType { get; set; }
        public string LicensePlate { get; set; }
        public bool IsActive { get; set; }
        public string CurrentLocation { get; set; } // e.g. "Lat,Lng"
    }

}
