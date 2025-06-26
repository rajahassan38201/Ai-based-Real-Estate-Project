using PakProperties.Models; // or the correct namespace where Sell and Rent are defined

namespace PakProperties.ViewModels
{
    public class PendingApprovalsViewModel
    {
        public List<Sell> PendingSells { get; set; }
        public List<Rent> PendingRents { get; set; }

        public List<Video> PendingVideos { get; set; }
    }

}
