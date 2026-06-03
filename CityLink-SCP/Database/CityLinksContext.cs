using CityLink_SCP.DbModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CityLink_SCP.Database;

/// <summary>
/// Inherits IdentityDbContext so all ASP.NET Identity tables
/// (AspNetUsers, AspNetRoles, AspNetUserRoles, etc.) are included
/// in the same SQLite database alongside the application tables.
/// 
/// ApplicationUser is the TUser type argument — Identity will use
/// our extended user class throughout.
/// </summary>
public class CityLinksContext : IdentityDbContext<ApplicationUser>
{
	// Application DbSets — Users/Staff are now queried via Identity's
	// UserManager, but we expose typed sets for direct EF queries.
	public DbSet<ApplicationUser> AppUsers => Set<ApplicationUser>();
	public DbSet<ApplicationStaff> AppStaff => Set<ApplicationStaff>();
	public DbSet<XmlConfig> XML_Configurations => Set<XmlConfig>();
	public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
	public DbSet<ServiceBooking> ServiceBookings => Set<ServiceBooking>();
	public DbSet<Event> Events => Set<Event>();
	public DbSet<Service> Services => Set<Service>();
	public DbSet<Feedback> Feedbacks => Set<Feedback>();

	public string DbPath { get; private set; } = string.Empty;

	public CityLinksContext(DbContextOptions<CityLinksContext> options) : base(options) { }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// Only configure SQLite if no options were already provided by DI
		// (prevents the double-configuration bug when AddDbContext is used)
		if (!optionsBuilder.IsConfigured)
		{
			var folder = System.IO.Directory.GetCurrentDirectory();
			DbPath = System.IO.Path.Join(folder, "Database", "CityLinks.db");
			optionsBuilder.UseSqlite($"Data Source={DbPath}");
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// MUST call base first — sets up all Identity table mappings
		base.OnModelCreating(modelBuilder);

		//  TPH discriminator for ApplicationUser / ApplicationStaff ─
		// EF Core infers TPH automatically for the inheritance hierarchy,
		// but we name the discriminator explicitly for clarity.
		modelBuilder.Entity<ApplicationUser>()
			.HasDiscriminator<string>("UserType")
			.HasValue<ApplicationUser>("User")
			.HasValue<ApplicationStaff>("Staff");

		//  Composite keys 
		modelBuilder.Entity<EventRegistration>()
			.HasKey(er => new { er.UserId, er.EventId });

		modelBuilder.Entity<ServiceBooking>()
			.HasKey(sb => new { sb.UserId, sb.ServiceId });

		//  XmlConfig → ApplicationStaff ─
		modelBuilder.Entity<XmlConfig>()
			.HasOne(x => x.Staff)
			.WithMany(s => s.XML_Configurations)
			.HasForeignKey(x => x.StaffId)
			.OnDelete(DeleteBehavior.Restrict);

		//  Event → ApplicationStaff 
		modelBuilder.Entity<Event>()
			.HasOne(e => e.Staff)
			.WithMany(s => s.Events)
			.HasForeignKey(e => e.StaffId)
			.OnDelete(DeleteBehavior.Restrict);

		//  Service → ApplicationStaff 
		modelBuilder.Entity<Service>()
			.HasOne(s => s.Staff)
			.WithMany(st => st.Services)
			.HasForeignKey(s => s.StaffId)
			.OnDelete(DeleteBehavior.Restrict);

		//  Feedback → ApplicationUser & ApplicationStaff ─
		modelBuilder.Entity<Feedback>()
			.HasOne(f => f.User)
			.WithMany(u => u.Feedbacks)
			.HasForeignKey(f => f.UserId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.SetNull);

		modelBuilder.Entity<Feedback>()
			.HasOne(f => f.Staff)
			.WithMany()
			.HasForeignKey(f => f.StaffId)
			.OnDelete(DeleteBehavior.Restrict);

		//  EventRegistration → ApplicationUser ─
		modelBuilder.Entity<EventRegistration>()
			.HasOne(er => er.User)
			.WithMany(u => u.EventRegistrations)
			.HasForeignKey(er => er.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<EventRegistration>()
			.HasOne(er => er.Event)
			.WithMany(e => e.EventRegistrations)
			.HasForeignKey(er => er.EventId)
			.OnDelete(DeleteBehavior.Cascade);

		//  ServiceBooking → ApplicationUser 
		modelBuilder.Entity<ServiceBooking>()
			.HasOne(sb => sb.User)
			.WithMany(u => u.ServiceBookings)
			.HasForeignKey(sb => sb.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<ServiceBooking>()
			.HasOne(sb => sb.Service)
			.WithMany(s => s.ServiceBookings)
			.HasForeignKey(sb => sb.ServiceId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
