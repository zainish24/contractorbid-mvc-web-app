using System.Collections.Generic;
using ContractorDashboard.Models;

namespace ContractorDashboard.ViewModels
{
    public class AdminDashboardStats
    {
        public int TotalContractors { get; set; }
        public int TotalJobs { get; set; }
        public int TotalBids { get; set; }
        public int PendingJobs { get; set; }
        public int AcceptedBids { get; set; }
        public int PendingBids { get; set; }
        public int RejectedBids { get; set; }
        public int UnderReviewBids { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public AdminDashboardStats Stats { get; set; } = new AdminDashboardStats();
        public List<Job> RecentJobs { get; set; } = new List<Job>();
        public List<Bid> RecentBids { get; set; } = new List<Bid>();
    }
}