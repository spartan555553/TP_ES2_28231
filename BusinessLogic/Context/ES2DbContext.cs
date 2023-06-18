using System;
using System.Collections.Generic;
using BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Context;

public partial class ES2DbContext : DbContext
{
    public ES2DbContext()
    {
    }

    public ES2DbContext(DbContextOptions<ES2DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<FixedDeposit> FixedDeposits { get; set; }

    public virtual DbSet<InvestmentFund> InvestmentFunds { get; set; }

    public virtual DbSet<MonthlyInterestRate> MonthlyInterestRates { get; set; }

    public virtual DbSet<RentedProperty> RentedProperties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=es2;Username=postgres;password=123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("Assets_pkey");

            entity.HasIndex(e => e.UserId, "fki_fk_user_id");

            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.AssetType)
                .HasMaxLength(255)
                .HasColumnName("asset_type");
            entity.Property(e => e.DurationInMonths).HasColumnName("duration_in_months");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.TaxPercentage)
                .HasPrecision(5, 2)
                .HasColumnName("tax_percentage");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Assets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_id");
        });

        modelBuilder.Entity<FixedDeposit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FixedDeposits_pkey");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountHolders)
                .HasMaxLength(255)
                .HasColumnName("account_holders");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(255)
                .HasColumnName("account_number");
            entity.Property(e => e.AnnualInterestRate)
                .HasPrecision(5, 2)
                .HasColumnName("annual_interest_rate");
            entity.Property(e => e.Bank)
                .HasMaxLength(255)
                .HasColumnName("bank");
            entity.Property(e => e.Value)
                .HasPrecision(10, 2)
                .HasColumnName("value");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.FixedDeposit)
                .HasForeignKey<FixedDeposit>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assets_id");
        });

        modelBuilder.Entity<InvestmentFund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("InvestmentFunds_pkey");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DefaultInterestRate)
                .HasPrecision(5, 2)
                .HasColumnName("default_interest_rate");
            entity.Property(e => e.InvestmentAmount)
                .HasPrecision(10, 2)
                .HasColumnName("investment_amount");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.InvestmentFund)
                .HasForeignKey<InvestmentFund>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assets_id");
        });

        modelBuilder.Entity<MonthlyInterestRate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("MonthlyInterestRtes_pkey");

            entity.HasIndex(e => e.FundId, "fki_fk_fund_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FundId).HasColumnName("fund_id");
            entity.Property(e => e.InterestRate)
                .HasPrecision(5, 2)
                .HasColumnName("interest_rate");
            entity.Property(e => e.Month).HasColumnName("month");

            entity.HasOne(d => d.Fund).WithMany(p => p.MonthlyInterestRtes)
                .HasForeignKey(d => d.FundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_fund_id");
        });

        modelBuilder.Entity<RentedProperty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RentedProperties_pkey");

            entity.HasIndex(e => e.Id, "fki_fk_assets_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EstimatedAnnualExpenses)
                .HasPrecision(10, 2)
                .HasColumnName("estimated_annual_expenses");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.MonthlyCondominiumFee)
                .HasPrecision(10, 2)
                .HasColumnName("monthly_condominium_fee");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PropertyValue)
                .HasPrecision(10, 2)
                .HasColumnName("property_value");
            entity.Property(e => e.RentalValue)
                .HasPrecision(10, 2)
                .HasColumnName("rental_value");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.RentedProperty)
                .HasForeignKey<RentedProperty>(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_assets_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.UserStatus)
                .HasMaxLength(255)
                .HasColumnName("user_status");
            entity.Property(e => e.UserType)
                .HasMaxLength(255)
                .HasColumnName("user_type");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
