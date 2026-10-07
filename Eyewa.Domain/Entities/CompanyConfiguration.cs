using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eyewa.Domain.Entities
{
    [Table("CompanyConfigurations")]
    public class CompanyConfiguration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = "Eyewa";

        public string? CompanyLogoUrl { get; set; }

        public string? CompanyLogoBase64 { get; set; }

        [MaxLength(150)]
        public string? AppTitle { get; set; } = "Eyewa Admin Portal";

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
