
namespace EbillingV2.Models
{
    public class PatientBillingViewModel
    {
        public int HospitalId { get; set; }
        public int LocationId { get; set; }
        public int SubLocationId { get; set; }
        public string Location { get; set; }
        public string PatSurname { get; set; }
        public string PatFirstname { get; set; }
        public string Sex { get; set; }
        public string MedAidName { get; set; }
        public string AttDoc { get; set; }
        public string SubLocation { get; set; }
        public string PatRefNo { get; set; }
        public string Letter { get; set; }

        public string Barcode { get; set; }
        public string Scan { get; set; }
    }


}