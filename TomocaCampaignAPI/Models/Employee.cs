using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TomocaCampaignAPI.Models
{
    public class Employee
    {
        [Key] // Specifies that this is the primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required String EmployeeId { get; set; }

        [Required]
        public String? EmployeCode { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public required string? ReferralCode { get; set; }

        [Required]
        public int ReferralCount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalRevenue { get; set; } = 0.0M;
    }
}
