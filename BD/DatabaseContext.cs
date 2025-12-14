using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BD
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Countries> Countries { get; set; }
        public DbSet<Sports> Sports { get; set; }
        public DbSet<Participants> Participants { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Venues> Venues { get; set; }
        public DbSet<Results> Results { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=dev;Username=postgres;Password=123");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка уникальных ключей и ограничений
            modelBuilder.Entity<Participants>()
                .HasIndex(p => new { p.CountryId, p.FullName })
                .IsUnique();

            modelBuilder.Entity<Results>()
                .HasIndex(r => new { r.SportId, r.ParticipantId })
                .IsUnique();
        }
    }

    // Таблица: countries
    [Table("countries")]
    public class Countries
    {
        [Key]
        [Column("country_id")]
        public int CountryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; }

        // Навигационное свойство
        public virtual ICollection<Participants> Participants { get; set; }
    }

    // Таблица: sports
    [Table("sports")]
    public class Sports
    {
        [Key]
        [Column("sport_id")]
        public int SportId { get; set; }

        [Required]
        [MaxLength(120)]
        [Column("name")]
        public string Name { get; set; }

        [Column("is_team")]
        public bool IsTeam { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // Навигационные свойства
        public virtual ICollection<Participants> Participants { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; }
        public virtual ICollection<Results> Results { get; set; }
    }

    // Таблица: venues
    [Table("venues")]
    public class Venues
    {
        [Key]
        [Column("venue_id")]
        public int VenueId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("location")]
        public string Location { get; set; }

        // Навигационное свойство
        public virtual ICollection<Schedule> Schedules { get; set; }
    }

    // Таблица: participants
    [Table("participants")]
    public class Participants
    {
        [Key]
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Required]
        [Column("country_id")]
        public int CountryId { get; set; }

        [Required]
        [Column("sport_id")]
        public int SportId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required]
        [Column("birth_date")]
        public DateTime BirthDate { get; set; }

        [MaxLength(1)]
        [Column("gender")]
        public string Gender { get; set; }

        // Навигационные свойства (внешние ключи)
        [ForeignKey("CountryId")]
        public virtual Countries Country { get; set; }

        [ForeignKey("SportId")]
        public virtual Sports Sport { get; set; }

        public virtual ICollection<Results> Results { get; set; }
    }

    // Таблица: schedule
    [Table("schedule")]
    public class Schedule
    {
        [Key]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("sport_id")]
        public int SportId { get; set; }

        [Required]
        [Column("venue_id")]
        public int VenueId { get; set; }

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        // Навигационные свойства (внешние ключи)
        [ForeignKey("SportId")]
        public virtual Sports Sport { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venues Venue { get; set; }
    }

    // Таблица: results
    [Table("results")]
    public class Results
    {
        [Key]
        [Column("result_id")]
        public int ResultId { get; set; }

        [Required]
        [Column("sport_id")]
        public int SportId { get; set; }

        [Required]
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [Column("place")]
        public int? Place { get; set; }

        [Column("score")]
        public decimal Score { get; set; }

        // Навигационные свойства (внешние ключи)
        [ForeignKey("SportId")]
        public virtual Sports Sport { get; set; }

        [ForeignKey("ParticipantId")]
        public virtual Participants Participant { get; set; }
    }
}
