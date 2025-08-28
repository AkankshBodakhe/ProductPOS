using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSLibrary.Entities
{
    public class SaleItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; } // FK to Sale

        [ForeignKey("SaleId")]
        public virtual Sale Sale { get; set; }

        [Required]
        public int ProductId { get; set; } // FK to Product

        [Required, StringLength(100)]
        public string ProductName { get; set; } // Snapshot of product name

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; } // Price per unit

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } // Can be overridden at billing

        [Range(0, double.MaxValue)]
        public decimal FinalPrice { get; set; } // (Rate * Qty) - Discount
    }
}
