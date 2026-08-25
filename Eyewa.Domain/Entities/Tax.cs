namespace Eyewa.Domain.Entities
{
    public class Tax
    {
        public int TaxId { get; set; }
        public string TaxName { get; set; } = "";
        public string TaxCode { get; set; } = "";
        public decimal TaxRate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
