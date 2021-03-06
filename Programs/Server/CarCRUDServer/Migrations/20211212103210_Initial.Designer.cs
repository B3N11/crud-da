// <auto-generated />
using System;
using CarCRUD.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CarCRUD.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("20211212103210_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.5")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("CarCRUD.DataModels.CarBrand", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("CarBrands");
                });

            modelBuilder.Entity("CarCRUD.DataModels.CarFavourite", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("carTypeDataID")
                        .HasColumnType("int");

                    b.Property<string>("color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("fuel")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("userDataID")
                        .HasColumnType("int");

                    b.Property<int>("year")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("carTypeDataID");

                    b.HasIndex("userDataID");

                    b.ToTable("FavouriteCars");
                });

            modelBuilder.Entity("CarCRUD.DataModels.CarImage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("favouriteCarDataID")
                        .HasColumnType("int");

                    b.Property<byte[]>("image")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("ID");

                    b.HasIndex("favouriteCarDataID");

                    b.ToTable("CarImages");
                });

            modelBuilder.Entity("CarCRUD.DataModels.CarType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("brandDataID")
                        .HasColumnType("int");

                    b.Property<string>("name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.HasIndex("brandDataID");

                    b.ToTable("CarType");
                });

            modelBuilder.Entity("CarCRUD.DataModels.UserData", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("active")
                        .HasColumnType("bit");

                    b.Property<string>("fullname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("passwordAttempts")
                        .HasColumnType("int");

                    b.Property<int>("type")
                        .HasColumnType("int");

                    b.Property<string>("username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CarCRUD.DataModels.UserRequest", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("brandAttach")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("type")
                        .HasColumnType("int");

                    b.Property<int?>("userDataID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("userDataID");

                    b.ToTable("UserRequests");
                });

            modelBuilder.Entity("CarCRUD.DataModels.CarFavourite", b =>
                {
                    b.HasOne("CarCRUD.DataModels.CarType", "carTypeData")
                        .WithMany()
                        .HasForeignKey("carTypeDataID");

                    b.HasOne("CarCRUD.DataModels.UserData", "userData")
                        .WithMany()
                        .HasForeignKey("userDataID");

                    b.Navigation("carTypeData");

                    b.Navigation("userData");
                });

            modelBuilder.Entity("CarCRUD.DataModels.CarImage", b =>
                {
                    b.HasOne("CarCRUD.DataModels.CarFavourite", "favouriteCarData")
                        .WithMany()
                        .HasForeignKey("favouriteCarDataID");

                    b.Navigation("favouriteCarData");
                });

            modelBuilder.Entity("CarCRUD.DataModels.CarType", b =>
                {
                    b.HasOne("CarCRUD.DataModels.CarBrand", "brandData")
                        .WithMany()
                        .HasForeignKey("brandDataID");

                    b.Navigation("brandData");
                });

            modelBuilder.Entity("CarCRUD.DataModels.UserRequest", b =>
                {
                    b.HasOne("CarCRUD.DataModels.UserData", "userData")
                        .WithMany()
                        .HasForeignKey("userDataID");

                    b.Navigation("userData");
                });
#pragma warning restore 612, 618
        }
    }
}
