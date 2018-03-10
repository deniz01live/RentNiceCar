using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentNiceCar.Models
{
    public class ViewCarsModel
    {
        public IEnumerable<Car> Cars;
        public bool NuBeschikbaar{ get; set; }
        public DateTime? VanafDatum { get; set; }
        public DateTime? TotDatum { get; set; }
        public string Error { get; set; }
    }

    public class Car
    {
        [Key]
        public string Kenteken { get; set; }
        public string Merk { get; set; }
        public string Type { get; set; }
        public string Soort { get; set; }
        public string Omschrijving { get; set; }
        public string GPS { get; set; }
        [DataType(DataType.Currency)]
        public float Prijs { get; set; }
        [DataType(DataType.Currency)]
        public float Borg { get; set; }
    }

    public class OrderCarViewModel
    {
        [Required]
        public string Kenteken { get; set; }
        [Required]
        [Display(Name = "Vanaf welke datum?")]
        public DateTime VanafDatum { get; set; }
        [Required]
        [Display(Name = "Tot welke datum?")]
        public DateTime TotDatum { get; set; }        
        public DateTime? InleverDatum { get; set; }
        public string Error { get; set; }
    }

}