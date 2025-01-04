﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Db.PodcastMgt.EF.Net9.Models;

public partial class PodcastMgtContext : DbContext
{
    public PodcastMgtContext()
    {
    }

    public PodcastMgtContext(DbContextOptions<PodcastMgtContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DnLd> DnLds { get; set; }

    public virtual DbSet<DnldStatus> DnldStatuses { get; set; }

    public virtual DbSet<Feed> Feeds { get; set; }

    public virtual DbSet<Machine> Machines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SqlExpress;Database=PodcastMgt;Trusted_Connection=True;TrustServerCertificate=Yes;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DnLd>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Hists2");

            entity.Property(e => e.AvailableLastDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.CastFilenameExt)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.CastSummary)
                .HasMaxLength(2048)
                .IsUnicode(false);
            entity.Property(e => e.CastTitle)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.CastUrl)
                .HasMaxLength(320)
                .IsUnicode(false);
            entity.Property(e => e.DnldStatusId)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DownloadStart)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.DownloadedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.DownloadedByPc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DownloadedByPC");
            entity.Property(e => e.DownloadedToDir)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.ErrLog)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.IsStillOnline).HasDefaultValue(true);
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Note)
                .HasMaxLength(2000)
                .IsUnicode(false);
            entity.Property(e => e.PublishedAt).HasColumnType("datetime");
            entity.Property(e => e.RowAddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RowAddedByPc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RowAddedByPC");
            entity.Property(e => e.TrgFileSortPrefix)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.DnldStatus).WithMany(p => p.DnLds)
                .HasForeignKey(d => d.DnldStatusId)
                .HasConstraintName("FK_DnLds_DnldStatus");

            entity.HasOne(d => d.Feed).WithMany(p => p.DnLds)
                .HasForeignKey(d => d.FeedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DnLds_Feeds3");
        });

        modelBuilder.Entity<DnldStatus>(entity =>
        {
            entity.ToTable("DnldStatus", "lku");

            entity.Property(e => e.Id)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Feed>(entity =>
        {
            entity.Property(e => e.AcptblAgeDay).HasDefaultValue(92);
            entity.Property(e => e.AddedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CastQntNew).HasDefaultValue(600);
            entity.Property(e => e.CastQntTtl).HasDefaultValue(600);
            entity.Property(e => e.HostMachineId)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.IgnoreBefore)
                .HasDefaultValueSql("(((1)-(1))-(2000))")
                .HasColumnType("datetime");
            entity.Property(e => e.IsNewerFirst).HasDefaultValue(true);
            entity.Property(e => e.KbPerMin).HasDefaultValue(600);
            entity.Property(e => e.LastCastAt).HasColumnType("datetime");
            entity.Property(e => e.LastCheckedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.LastCheckedPc)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("LastCheckedPC");
            entity.Property(e => e.LatestRssText).HasColumnType("text");
            entity.Property(e => e.LatestRssXml).HasColumnType("xml");
            entity.Property(e => e.Name).HasMaxLength(128);
            entity.Property(e => e.Note).HasMaxLength(2000);
            entity.Property(e => e.PartSizeMin).HasDefaultValue(1.0);
            entity.Property(e => e.PriorityGroup).HasDefaultValue(10);
            entity.Property(e => e.StatusInfo)
                .HasMaxLength(32)
                .HasDefaultValue("n/a");
            entity.Property(e => e.SubFolder).HasMaxLength(128);
            entity.Property(e => e.Tla).HasMaxLength(28);
            entity.Property(e => e.Url).HasMaxLength(256);

            entity.HasOne(d => d.HostMachine).WithMany(p => p.Feeds)
                .HasForeignKey(d => d.HostMachineId)
                .HasConstraintName("FK_Feeds_Machine");
        });

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.ToTable("Machine");

            entity.Property(e => e.Id)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.TargetDrive)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("C")
                .IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
