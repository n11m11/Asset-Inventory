using System.ComponentModel.DataAnnotations.Schema;

namespace AssetInventory.Models
{
	[Table("offices")]
	public class Office
	{
		public int OfficeId { get; set; }
		public string Name { get; set; }
		public string Country { get; set; }

		public virtual ICollection<Asset> Assets { get; set; }
	}
}