namespace ContractorDashboard.Models
{
    public class ManusCalculation
    {
        public int Id { get; set; }
        public int BidId { get; set; }

        public string InputParameters { get; set; } = string.Empty;
        public string OutputResult { get; set; } = string.Empty;
        public decimal CalculatedAmount { get; set; }
        public string? Breakdown { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Bid? Bid { get; set; }
    }
}