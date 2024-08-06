using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AssetInventory.Models
{
	[PrimaryKey("Key")]
	[Table("http_cache")]
	public class HttpCache
	{
		public string Key { get; set; }
		public string Body { get; set; }
		public double UnixValidTo { get; set; }
	}
}