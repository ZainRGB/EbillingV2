namespace EbillingV2.Models
{
    public class StockLocationViewModel
    {
        public int StockLocationId { get; set; }      // sl.id
        public string LocationName { get; set; }      // sl.location
        public int LocationId { get; set; }           // sub.id
        public string SubLocationName { get; set; }   // sub.sublocation
        public int HospitalId { get; set; }   // sub.sublocation
    }
}
