using DAL.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore;
using System;

namespace DAL
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ticket> tickets { get; set; }
        public DbSet<Event> events { get; set; }
        public DbSet<Seat> seats { get; set; }
        public DbSet<Order> orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // سطر السحر: لازم تسيب السطر ده في الأول عشان يفرش جداول الـ Identity بالـ Guid صح
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                // الـ Id موروث كـ Guid خلاص
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.f_name).HasMaxLength(50).IsRequired();
                entity.Property(u => u.l_name).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Eid);
                entity.Property(e => e.Title).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Place).HasMaxLength(150).IsRequired();
            });

            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Number).HasMaxLength(10).IsRequired();
                entity.Property(s => s.Section).HasMaxLength(50).IsRequired();
                entity.Property(s => s.Row).HasMaxLength(10).IsRequired();
                entity.Property(s => s.Price).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

                entity.HasOne(s => s.Event)
                      .WithMany(e => e.Seats)
                      .HasForeignKey(s => s.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Oid);
                entity.Property(o => o.total_price).HasColumnType("decimal(18,2)");
                entity.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(20);
                entity.Property(o => o.PaymentGatewayTransactionId).HasMaxLength(100);

                // العلاقة دي دلوقتي هتربط Guid مع Guid بدون أي اعتراض!
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.Uid)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Seat)
                      .WithMany()
                      .HasForeignKey(o => o.sid)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Tid);
                entity.Property(t => t.TicketCode).HasMaxLength(100).IsRequired();
                entity.Property(t => t.TicketQR).IsRequired();

                entity.HasOne(t => t.order)
                      .WithOne(o => o.Ticket)
                      .HasForeignKey<Ticket>(t => t.Orderid)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(t => t.seat)
                      .WithOne()
                      .HasForeignKey<Ticket>(t => t.Seatid)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}