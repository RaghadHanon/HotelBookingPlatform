using HotelBookingPlatform.Domain.Enums;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence;
public class HotelBookingPlatformDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<BookingRoom> BookingRooms { get; set; }
    public DbSet<Guest> Guests { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<RoomImage> RoomImages { get; set; }
    public DbSet<HotelImage> HotelImages { get; set; }
    public DbSet<CityImage> CityImages { get; set; }

    public HotelBookingPlatformDbContext(DbContextOptions<HotelBookingPlatformDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
           .HasOne(a => a.Guest)
           .WithOne()
           .HasForeignKey<ApplicationUser>("GuestId");

        modelBuilder.Entity<Hotel>()
           .HasOne(h => h.Location)
           .WithOne()
           .HasForeignKey<Location>("HotelId");

        modelBuilder.Entity<BookingRoom>()
            .HasKey(br => new { br.BookingId, br.RoomId });

        modelBuilder.Entity<BookingRoom>()
            .HasOne(br => br.Booking)
            .WithMany(b => b.BookingRooms)
            .HasForeignKey(br => br.BookingId);

        modelBuilder.Entity<BookingRoom>()
            .HasOne(br => br.Room)
            .WithMany(r => r.BookingRooms)
            .HasForeignKey(br => br.RoomId);

        IgnoreComputedProperties(modelBuilder);
        SetPrecisionForFloatingPointTypes(modelBuilder);
        ConfigureDeleteBehavior(modelBuilder);
        AddIndexes(modelBuilder);
        SeedData(modelBuilder);
    }

    private void IgnoreComputedProperties(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>()
                    .Ignore(h => h.RoomsNumber);
        modelBuilder.Entity<Discount>()
                    .Ignore(d => d.OriginalPrice);
        modelBuilder.Entity<Discount>()
                    .Ignore(d => d.DiscountedPrice);
        modelBuilder.Entity<Guest>()
                    .Ignore(g => g.FullName);
        modelBuilder.Entity<BookingRoom>()
                    .Ignore(b => b.FinalPrice);
    }

    private void SetPrecisionForFloatingPointTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>()
                    .Property(r => r.Price)
                    .HasPrecision(18, 2);
        modelBuilder.Entity<Booking>()
                    .Property(b => b.Price)
                    .HasPrecision(18, 2);
        modelBuilder.Entity<Discount>()
                    .Property(d => d.Percentage)
                    .HasPrecision(18, 2);
        modelBuilder.Entity<Location>()
                    .Property(l => l.Latitude)
                    .HasPrecision(8, 6);
        modelBuilder.Entity<Location>()
                    .Property(l => l.Longitude)
                    .HasPrecision(9, 6);
    }

    private void ConfigureDeleteBehavior(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey> foreignKeys =
             modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior != DeleteBehavior.Restrict);

        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey? foreignKey in foreignKeys)
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.Entity<Room>()
            .HasMany(r => r.Images)
            .WithOne()
            .HasForeignKey(ri => ri.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<City>()
           .HasMany(c => c.Images)
           .WithOne()
           .HasForeignKey(ci => ci.CityId)
           .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Hotel>()
            .HasMany(h => h.Images)
            .WithOne()
            .HasForeignKey(hi => hi.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Hotel>()
            .HasOne(h => h.Location)
            .WithOne()
            .HasForeignKey<Location>("HotelId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>()
                    .HasMany(b => b.BookingRooms)
                    .WithOne(br => br.Booking)
                    .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Amenity>()
            .HasMany(a => a.Rooms)
            .WithMany(r => r.Amenities)
            .UsingEntity<Dictionary<string, object>>(
                "RoomAmenity",
                j => j.HasOne<Room>()
                      .WithMany()
                      .HasForeignKey("RoomId")
                      .OnDelete(DeleteBehavior.Cascade),

                j => j.HasOne<Amenity>()
                      .WithMany()
                      .HasForeignKey("AmenityId")
                      .OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder.Entity<Amenity>()
            .HasMany(a => a.Hotels)
            .WithMany(h => h.Amenities)
            .UsingEntity<Dictionary<string, object>>(
                "HotelAmenity",
                j => j.HasOne<Hotel>()
                      .WithMany()
                      .HasForeignKey("HotelId")
                      .OnDelete(DeleteBehavior.Cascade),

                j => j.HasOne<Amenity>()
                      .WithMany()
                      .HasForeignKey("AmenityId")
                      .OnDelete(DeleteBehavior.Cascade)
            );
    }

    private void AddIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Discount>()
            .HasIndex(d => new { d.StartDate, d.EndDate })
            .HasDatabaseName("IDX_Discounts_StartDate_EndDate");

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.RoomType)
            .HasDatabaseName("IDX_Room_Type");

        modelBuilder.Entity<Room>()
            .HasIndex(r => new { r.AdultsCapacity, r.ChildrenCapacity })
            .HasDatabaseName("IDX_Rooms_Adults_Children_Capacity");

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new { b.CheckInDate, b.CheckOutDate })
            .HasDatabaseName("IDX_Bookings_CheckInDate_CheckOutDate");

        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.StarRate)
            .HasDatabaseName("IDX_Hotels_StarRate");

        modelBuilder.Entity<City>()
            .HasIndex(c => new { c.Name, c.Country })
            .HasDatabaseName("IDX_Cities_Name_Country");

        modelBuilder.Entity<Hotel>()
            .HasIndex(h => new { h.Name, h.Description })
            .HasDatabaseName("IDX_Hotels_Name_Description");

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.Title, r.Description })
            .HasDatabaseName("IDX_Reviews_Title_Description");

        modelBuilder.Entity<Room>()
            .HasIndex(r => new { r.HotelId, r.RoomNumber })
            .IsUnique()
            .HasDatabaseName("IDX_Hotel_RoomNumber");
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        Guid newYorkId = new Guid("1183b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid londonId = new Guid("1283b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid parisId = new Guid("1383b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid tokyoId = new Guid("1483b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid berlinId = new Guid("1583b59c-f7f8-4b21-b1df-5149fb57984e");

        modelBuilder.Entity<City>().HasData(
            new City { Id = newYorkId, Name = "New York", Country = "USA", PostOffice = "10001", CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new City { Id = londonId, Name = "London", Country = "UK", PostOffice = "SW1A 1AA", CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new City { Id = parisId, Name = "Paris", Country = "France", PostOffice = "75001", CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new City { Id = tokyoId, Name = "Tokyo", Country = "Japan", PostOffice = "100-0001", CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new City { Id = berlinId, Name = "Berlin", Country = "Germany", PostOffice = "10115", CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) }
        );

        Guid grandHyattId = new Guid("2283b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid theRitzId = new Guid("2383b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid leMeridienId = new Guid("2483b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid tokyoPalaceId = new Guid("2583b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid berlinGrandId = new Guid("2683b59c-f7f8-4b21-b1df-5149fb57984e");

        Guid newYorkLocationId = new Guid("2783b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid londonLocationId = new Guid("2883b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid parisLocationId = new Guid("2983b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid tokyoLocationId = new Guid("3083b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid berlinLocationId = new Guid("3183b59c-f7f8-4b21-b1df-5149fb57984e");

        modelBuilder.Entity<Hotel>().HasData(
            new Hotel { Id = grandHyattId, Name = "Grand Hyatt", Owner = "Hyatt Group", StarRate = 5, Street = "Times Square, New York", CityId = newYorkId, CheckInTime = new TimeOnly(14, 0), CheckOutTime = new TimeOnly(12, 0), CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14), LocationId = newYorkLocationId },
            new Hotel { Id = theRitzId, Name = "The Ritz", Owner = "Ritz-Carlton", StarRate = 5, Street = "Piccadilly, London", CityId = londonId, CheckInTime = new TimeOnly(15, 0), CheckOutTime = new TimeOnly(11, 30), CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14), LocationId = londonLocationId },
            new Hotel { Id = leMeridienId, Name = "Le Méridien", Owner = "Marriott Group", StarRate = 4, Street = "Champs-Élysées, Paris", CityId = parisId, CheckInTime = new TimeOnly(14, 30), CheckOutTime = new TimeOnly(12, 30), CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14), LocationId = parisLocationId },
            new Hotel { Id = tokyoPalaceId, Name = "Tokyo Palace", Owner = "Palace Hotels", StarRate = 4, Street = "Chiyoda, Tokyo", CityId = tokyoId, CheckInTime = new TimeOnly(14, 0), CheckOutTime = new TimeOnly(12, 0), CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14), LocationId = tokyoLocationId },
            new Hotel { Id = berlinGrandId, Name = "Berlin Grand", Owner = "Grand Hotels", StarRate = 4, Street = "Mitte, Berlin", CityId = berlinId, CheckInTime = new TimeOnly(15, 0), CheckOutTime = new TimeOnly(11, 30), CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14), LocationId = berlinLocationId }
        );

        modelBuilder.Entity<Location>().HasData(
            new Location { Id = newYorkLocationId, Latitude = 40.7128, Longitude = -74.0060, HotelId = grandHyattId },
            new Location { Id = londonLocationId, Latitude = 51.5074, Longitude = -0.1278, HotelId = theRitzId },
            new Location { Id = parisLocationId, Latitude = 48.8566, Longitude = 2.3522, HotelId = leMeridienId },
            new Location { Id = tokyoLocationId, Latitude = 35.6762, Longitude = 139.6503, HotelId = tokyoPalaceId },
            new Location { Id = berlinLocationId, Latitude = 52.5200, Longitude = 13.4050, HotelId = berlinGrandId }
        );

        Guid room1GrandHyattId = new Guid("3283b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid room1TheRitzId = new Guid("3383b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid room1LeMeridienId = new Guid("3483b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid room1TokyoPalaceId = new Guid("3583b59c-f7f8-4b21-b1df-5149fb57984e");
        Guid room1BerlinGrandId = new Guid("3683b59c-f7f8-4b21-b1df-5149fb57984e");

        modelBuilder.Entity<Room>().HasData(
            new Room { Id = room1GrandHyattId, RoomNumber = 101, AdultsCapacity = 2, ChildrenCapacity = 1, Price = 300, HotelId = grandHyattId, RoomType = RoomType.Standard, CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new Room { Id = room1TheRitzId, RoomNumber = 101, AdultsCapacity = 3, ChildrenCapacity = 0, Price = 500, HotelId = theRitzId, RoomType = RoomType.Luxury, CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new Room { Id = room1LeMeridienId, RoomNumber = 101, AdultsCapacity = 2, ChildrenCapacity = 2, Price = 250, HotelId = leMeridienId, RoomType = RoomType.Boutique, CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new Room { Id = room1TokyoPalaceId, RoomNumber = 101, AdultsCapacity = 2, ChildrenCapacity = 1, Price = 350, HotelId = tokyoPalaceId, RoomType = RoomType.Standard, CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) },
            new Room { Id = room1BerlinGrandId, RoomNumber = 101, AdultsCapacity = 2, ChildrenCapacity = 0, Price = 450, HotelId = berlinGrandId, RoomType = RoomType.Boutique, CreationDate = new DateTime(2023, 12, 14), LastModified = new DateTime(2023, 12, 14) }
        );
    }
}
