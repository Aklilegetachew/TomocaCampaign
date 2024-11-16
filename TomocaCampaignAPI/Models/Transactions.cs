using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TomocaCampaignAPI.Models
{
    public class Transactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string EmployeeName { get; set; }

        [Required]
        [ForeignKey(nameof(Employee))]
        public int EmployeeDbId { get; set; } // Match the type of Employee primary key

        [Required]
        [ForeignKey(nameof(User))]
        public int UserDbId { get; set; } // Match the type of User primary key

        [Required]
        public string TransactionId { get; set; }

        [Required]
        public decimal TotalTransaction { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Employee Employee { get; set; }
        public User User { get; set; }
    }
}
