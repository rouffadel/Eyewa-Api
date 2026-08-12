namespace Eyewa.Domain.Entities
{
    public class TenantFeatureAccess
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public bool HasInsuranceAccess { get; set; }
        public bool HasRedmeePointsAccess { get; set; }
        public bool HasProductsAccess { get; set; }
    }
}
