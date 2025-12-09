using System.ComponentModel.DataAnnotations;

namespace ContractorDashboard.Models
{
    public class Contractor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties - FIXED TYPO
        public ContractorSettings? Settings { get; set; } // Changed from ContractorSettings to Settings
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }
}