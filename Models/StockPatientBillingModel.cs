namespace EbillingV2.Models
{
    public class StockPatientBillingModel
    {
        public int id { get; set; }
        public string patrefno { get; set; }
        public string? patsurname { get; set; }
        public string? patfirstname { get; set; }
        public int locationid { get; set; }
        public string stockcode { get; set; }
        public string? packsize { get; set; }
        public int stockquantity { get; set; } = 1;
        public string tblday { get; set; }
        public string tblmonth { get; set; }
        public string tblyear { get; set; }
        public string tbltime { get; set; }
        public string description { get; set; }
        public string? active { get; set; }
        public string status { get; set; }
        public string? type { get; set; }
        public int hospitalid { get; set; }
        public int sublocationid { get; set; }
    }
}
