namespace hotelressys.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., Single, Double
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}