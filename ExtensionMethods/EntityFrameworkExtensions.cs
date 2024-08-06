using System.Diagnostics.CodeAnalysis;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Asset_Inventory.ExtensionMethods
{
    internal static class EntityFrameworkExtensions
    {
        public static int SaveChangesWithStatusAndLog(this DbContext dbContext)
        {
            //bool acceptAllChangesOnSuccess = ...
            int countWritten = SpectreHelper.Loading().Start("Calling SaveChanges()", _ => dbContext.SaveChanges());
            Console.WriteLine(countWritten + (countWritten == 1 ? " row changed." : " rows changed."));
            return countWritten;
        }
        public static bool PreviouslyMigrated(this DatabaseFacade db)
        {
            // InMemory database provider cannot have migrations.
            if (db.IsInMemory())
                return false;
            // for non-relational, you must use a different approach.
            if (!db.IsRelational())
                return false;
            // check if migrations have previously been applied.
            return db.GetAppliedMigrations().Any();
        }
        public static bool TestConnection(this DatabaseFacade db, out Exception? ex)
        {
            ex = null;
            if (db.IsInMemory())
                return true;
            bool success = true;
            if (!db.CanConnect())
                success = false;
            else
            {
                try
                {
                    db.OpenConnection();
                    db.CloseConnection();
                }
                catch (Exception _ex)
                {
                    success = false;
                    ex = _ex;
                }
            }
            return success;
        }
        public static bool TryCreate(this DatabaseFacade db, bool migrate, [NotNullWhen(false)] out Exception? ex)
        {
            ex = null;
            bool success = true;
            try
            {
                if (migrate)
                {
                    db.Migrate();
                    if (!db.GetAppliedMigrations().Any())
                        throw new Exception("No migrations have been applied. This will result in errors.");
                }
                else
                {
                    db.EnsureCreated();
                }
            }
            catch (Exception _ex)
            {
                ex = _ex;
                success = false;
            }
            return success;
        }

    }
}