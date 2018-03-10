using System;
namespace RentNiceCar.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string Kenteken { get; set; }
        public string UserId { get; set; }
        public DateTime VanafDatum { get; set; }
        public DateTime TotDatum { get; set; }
        public DateTime? InleverDatum { get; set; }
        public DateTime Datum { get; set; }
    }
}