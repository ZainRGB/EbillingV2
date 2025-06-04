using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EbillingV2.Models
{

    [Table("tblsignboards")]
    public class Patient
    {
        [Key]
        //public int Id { get; set; }
        //public string Letter { get; set; }
        //public string PatRefNo { get; set; }
        //public string Active { get; set; }
        //public string PatSurname { get; set; }
        //public int HospitalId { get; set; }
        //public int LocationId { get; set; }
        //public int SubLocationId { get; set; }
        //public string Location { get; set; }
        //public string SubLocation { get; set; }

        [Column("id")] // Map to actual column name
        public int Id { get; set; }

        [Column("patrefno")]
        public string PatRefNo { get; set; }

        [Column("patsurname")]
        public string PatSurname { get; set; }

        [Column("patfirstname")]
        public string PatFirstname { get; set; }

        [Column("attdoc")]
        public string AttDoc { get; set; }
        public string Sex { get; set; }

        [Column("medaidname")]
        public string MedAidName { get; set; }

        [Column("hospitalid")]
        public int HospitalId { get; set; }

        [Column("locationid")]
        public int LocationId { get; set; } // Keep as string if DB stores it as text

        [Column("sublocationid")]
        public int SubLocationId { get; set; }

        [Column("active")]
        public string Active { get; set; } = "";


    }
    // Navigation properties if needed
    // public Hospital Hospital { get; set; }
    // public Location Location { get; set; }

}
