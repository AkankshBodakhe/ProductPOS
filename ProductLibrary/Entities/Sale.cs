
using POSLibrary.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSLibrary.Entities
{
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto increment Invoice No
        public int Id { get; set; } // Invoice Number

        [Required]
        public DateTime SaleDate { get; set; }

        [Required]
        public int SalesmanId { get; set; } // FK to User

        [Required, StringLength(100)]
        public string SalesmanName { get; set; } // Snapshot of who sold

        [Required, StringLength(100)]
        public string CustomerName { get; set; }

        [StringLength(15)]
        public string CustomerContact { get; set; }

        [Required]
        public PaymentMode PaymentMode { get; set; }

        [Range(0, double.MaxValue)]
        public decimal GrandTotal { get; set; }

        // Navigation property
        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }
}
