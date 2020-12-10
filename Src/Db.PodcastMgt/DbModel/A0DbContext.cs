namespace Db.PodcastMgt.DbModel
{
    using System.Data.Entity;

    public partial class A0DbContext : DbContext
  {
    public virtual DbSet<DnLd> DnLds { get; set; }
    public virtual DbSet<Feed> Feeds { get; set; }
    public virtual DbSet<Machine> Machines { get; set; }
    public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
    public virtual DbSet<DnldStatu> DnldStatus { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Entity<DnLd>()
          .Property(e => e.CastTitle)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.CastSummary)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.CastUrl)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.CastFilenameExt)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.TrgFileSortPrefix)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.RowAddedByPC)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.DownloadedByPC)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.DownloadedToDir)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.DnldStatusId)
          .IsFixedLength()
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.Note)
          .IsUnicode(false);

      modelBuilder.Entity<DnLd>()
          .Property(e => e.ErrLog)
          .IsUnicode(false);

      modelBuilder.Entity<Feed>()
          .Property(e => e.LatestRssText)
          .IsUnicode(false);

      modelBuilder.Entity<Feed>()
          .Property(e => e.LastCheckedPC)
          .IsUnicode(false);

      modelBuilder.Entity<Feed>()
          .Property(e => e.HostMachineId)
          .IsUnicode(false);

      modelBuilder.Entity<Feed>()
          .HasMany(e => e.DnLds)
          .WithRequired(e => e.Feed)
          .WillCascadeOnDelete(false);

      modelBuilder.Entity<Machine>()
          .Property(e => e.Id)
          .IsUnicode(false);

      modelBuilder.Entity<Machine>()
          .Property(e => e.Note)
          .IsUnicode(false);

      modelBuilder.Entity<Machine>()
          .Property(e => e.TargetDrive)
          .IsFixedLength()
          .IsUnicode(false);

      modelBuilder.Entity<Machine>()
          .HasMany(e => e.Feeds)
          .WithOptional(e => e.Machine)
          .HasForeignKey(e => e.HostMachineId);

      modelBuilder.Entity<DnldStatu>()
          .Property(e => e.Id)
          .IsFixedLength()
          .IsUnicode(false);

      modelBuilder.Entity<DnldStatu>()
          .Property(e => e.Name)
          .IsUnicode(false);

      modelBuilder.Entity<DnldStatu>()
          .Property(e => e.Description)
          .IsUnicode(false);

      modelBuilder.Entity<DnldStatu>()
          .HasMany(e => e.DnLds)
          .WithOptional(e => e.DnldStatu)
          .HasForeignKey(e => e.DnldStatusId);
    }
  }
}
