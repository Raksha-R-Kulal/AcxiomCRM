using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Lead
    {
        public int LeadId { get; set; }

        public string LeadCode { get; set; } = "";

        [Required(ErrorMessage = "Lead name is required.")]
        [StringLength(100)]
        public string LeadName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string Phone { get; set; } = "";

        [StringLength(100)]
        public string? CompanyName { get; set; }

        [Required]
        public string Source { get; set; } = "Website";

        [Required]
        public string Status { get; set; } = "New";

        [Range(0, 100000000)]
        public decimal ExpectedValue { get; set; }

        public string? AssignedToUserId { get; set; }

        public string? OwnerId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsConverted { get; set; } = false;

        public int? ConvertedCustomerId { get; set; }
    }
}