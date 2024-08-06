using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AssetInventory.Models
{
    
    [Table("assets")]
    public class Asset()
    {
        public int Id { get; set; }
        public AssetType Type { get; set; } = AssetType.Unknown;
        public string ModelName { get; set; } = string.Empty;
        /// <summary>
        /// Should be rounded to 2 decimal points manually.
        /// </summary>
        [Column("Price",TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; } = null;

        [Column(TypeName = "nvarchar(16)")]
        public string PriceCurrency { get; set; } = string.Empty;
        public DateTime? PurchaseDate { get; set; }
        public int OfficeId { get; set; }

        public virtual Office Office { get; set; }

    }
}