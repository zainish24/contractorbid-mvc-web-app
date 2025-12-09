using System.ComponentModel.DataAnnotations;

namespace ContractorDashboard.Models
{
    public class ContractorSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractorId { get; set; }

        [Range(0, 1000)]
        public decimal LaborRate { get; set; } = 45.00m;

        [Range(0, 100)]
        public decimal MaterialMargin { get; set; } = 15.00m;

        [Range(0, 500)]
        public decimal TravelCost { get; set; } = 25.00m;

        [Range(0, 50)]
        public decimal ProfitMargin { get; set; } = 20.00m;

        [StringLength(500)]
        public string PreferredLocations { get; set; } = string.Empty;

        public bool AutoCalculateBids { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Contractor? Contractor { get; set; }
    }
}