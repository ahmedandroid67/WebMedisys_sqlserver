namespace Cabinet.Models
{
    public class CabinetInfo
    {
        public int Id { get; set; }
        public string? DrName { get; set; }      // Displayed on Ordonnance
        public string? Speciality { get; set; }  // Displayed under name
        public string? Address { get; set; }     // Cabinet location
        public string? Phone { get; set; }       // Contact number
        public string? Email { get; set; }       // Professional email
    }
}