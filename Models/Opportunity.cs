using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Opportunity
    {
        public int OpportunityId { get; set; }

        [Required(ErrorMessage = "Opportunity name is required.")]
        [StringLength(100)]
        public string OpportunityName { get; set; } = "";

        // Linked Customer
        [Required]
        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        // Linked Lead (optional)
        public int? LeadId { get; set; }

        public Lead? Lead { get; set; }

        // Deal Amount
        [Required]
        [Range(1, 1000000000)]
        public decimal DealAmount { get; set; }

        // Probability percentage
        [Range(0, 100)]
        public int Probability { get; set; } = 50;

        // Expected Closing Date
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpectedCloseDate { get; set; }

        // Sales Stage
        [Required]
        public string Stage { get; set; } = "Qualification";

        public string? OwnerId { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}