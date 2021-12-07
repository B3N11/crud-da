using Microsoft.EntityFrameworkCore;
using CarCRUD.DataModels;

namespace CarCRUD.DataBase
{
    public class Context : DbContext
    {
        public DbSet<UserData> Users { get; set; }
        public DbSet<UserBrandRequest> UserRequests { get; set; }
        public DbSet<CarBrand> CarBrands { get; set; }
        public DbSet<CarType> CarType { get; set; }
        public DbSet<CarFavourite> FavouriteCars { get; set; }
        public DbSet<CarImage> CarImages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=CarCRUD;Trusted_Connection=True;");
        }
    }
}