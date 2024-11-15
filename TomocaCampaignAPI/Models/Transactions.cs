using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
        public String EmployeeName { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public String EmployeId { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserID { get; set; }

        [Required]
        public string transactionId { get; set; }

        [Required]
        public decimal TotalTransaction { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Employee Employee { get; set; }
        public Employee User { get; set; }

    }
}