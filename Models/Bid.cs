using System.ComponentModel.DataAnnotations;

namespace ContractorDashboard.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ContractorId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Range(0, 1000000)]
        public decimal BidAmount { get; set; }

        public string? Notes { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Submitted";

        public string? CalculationDetails { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties - FIXED
        public Contractor? Contractor { get; set; }
        public Job? Job { get; set; }
        public ManusCalculation? ManusCalculation { get; set; } // ADD THIS LINE
    }
}