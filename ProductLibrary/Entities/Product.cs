using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSLibrary.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // User provides product Id manually
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Rate { get; set; } // Price per unit

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } // Default discount (can be overridden at billing)

        [Range(0, int.MaxValue)]
        public int AvailableStock { get; set; }
    }
}
