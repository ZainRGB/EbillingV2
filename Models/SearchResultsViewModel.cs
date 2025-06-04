using System.ComponentModel.DataAnnotations.Schema;

namespace EbillingV2.Models
{
    public class SearchResultsViewModel
    {
        public PatientBillingViewModel SearchParameters { get; set; }
        public List<PatientBillingViewModel> Results { get; set; }

        public List<StockResultViewModel> StockResults { get; set; } = new();

    }
}
