using CityLink_SCP.DbModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace CityLink_SCP.Database;

public class CityLinksContext : DbContext
{
	public DbSet<User> Users => Set<User>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<XML_Configurations> XML_Configurations => Set<XML_Configurations>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<ServiceBooking> ServiceBookings => Set<ServiceBooking>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public string DbPath { get; private set; }

    public CityLinksContext(DbContextOptions<CityLinksContext> options) : base(options)
    {

	}
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var folder = System.IO.Directory.GetCurrentDirectory();
		DbPath = System.IO.Path.Join(folder, "Database", "CityLinks.db");
		optionsBuilder.UseSqlite($"Data Source={DbPath}");
	}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships and constraints between the tables
        modelBuilder.Entity<XML_Configurations>()
            .HasOne(x => x.Staff)
            .WithMany()
            .HasForeignKey(x => x.StaffId);
        modelBuilder.Entity<EventRegistration>()
            .HasKey(er => new { er.UserId, er.EventId });
        modelBuilder.Entity<ServiceBooking>()
            .HasKey(sb => new { sb.UserId, sb.ServiceId });
    }
}
