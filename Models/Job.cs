using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace ContractorDashboard.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        public string BudgetRange { get; set; } = string.Empty;

        [StringLength(50)]
        public string JobType { get; set; } = "Construction"; // Construction, Renovation, Repair, etc.

        public decimal? EstimatedHours { get; set; }
        public string? RequiredMaterials { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, InProgress, Completed, Cancelled

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
        public DateTime? Deadline { get; set; }

        // Navigation properties
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}