using AssetInventory.Data;
using AssetInventory.Logic;
using AssetInventory.Models;

namespace AssetInventory.Menus
{
	internal partial class _MenusClass
	{
		public AssetInventoryDbContext ctx;
		public QueryChain<IQueryable<Asset>> assetQueryChain;
	}
}