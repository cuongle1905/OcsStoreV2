using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace OcsStore.Models;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillDetail> BillDetails { get; set; }

    public virtual DbSet<BillDetailView> BillDetailViews { get; set; }

    public virtual DbSet<BillView> BillViews { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerDebtView> CustomerDebtViews { get; set; }

    public virtual DbSet<CustomerView> CustomerViews { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemGroup> ItemGroups { get; set; }

    public virtual DbSet<ItemView> ItemViews { get; set; }

    public virtual DbSet<LastStoreTransaction> LastStoreTransactions { get; set; }

    public virtual DbSet<Param> Params { get; set; }

    public virtual DbSet<Processing> Processings { get; set; }

    public virtual DbSet<ProcessingDetail> ProcessingDetails { get; set; }

    public virtual DbSet<ProcessingDetailView> ProcessingDetailViews { get; set; }

    public virtual DbSet<ProcessingModel> ProcessingModels { get; set; }

    public virtual DbSet<ProcessingModelDetail> ProcessingModelDetails { get; set; }

    public virtual DbSet<ProcessingModelDetailView> ProcessingModelDetailViews { get; set; }

    public virtual DbSet<ProcessingType> ProcessingTypes { get; set; }

    public virtual DbSet<ProfitView> ProfitViews { get; set; }

    public virtual DbSet<Receiving> Receivings { get; set; }

    public virtual DbSet<ReceivingDetail> ReceivingDetails { get; set; }

    public virtual DbSet<ReceivingDetailView> ReceivingDetailViews { get; set; }

    public virtual DbSet<StockCardView> StockCardViews { get; set; }

    public virtual DbSet<StockView> StockViews { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreTransaction> StoreTransactions { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("name=MySqlConnection", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.3.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bill");

            entity.HasIndex(e => e.Customer, "fk_bill_customer_idx");

            entity.HasIndex(e => new { e.Date, e.Time }, "fk_bill_date_time");

            entity.HasIndex(e => e.Store, "fk_bill_store_idx");

            entity.HasIndex(e => e.UserCreated, "fk_bill_user_created_idx");

            entity.HasIndex(e => e.UserModified, "fk_bill_user_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Customer)
                .HasDefaultValueSql("'1'")
                .HasColumnName("customer");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(200)
                .HasColumnName("customer_address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(50)
                .HasColumnName("customer_email")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasColumnName("customer_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(15)
                .HasColumnName("customer_phone");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("datetime")
                .HasColumnName("date_modified");
            entity.Property(e => e.DatePaid)
                .HasColumnType("datetime")
                .HasColumnName("date_paid");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Paid)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("paid");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.TimeCreated)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time_created");
            entity.Property(e => e.TimeModified)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time_modified");
            entity.Property(e => e.TimePaid)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time_paid");
            entity.Property(e => e.TotalValue)
                .HasPrecision(10, 2)
                .HasColumnName("total_value");
            entity.Property(e => e.UserCreated).HasColumnName("user_created");
            entity.Property(e => e.UserModified)
                .HasDefaultValueSql("'1'")
                .HasColumnName("user_modified");
            entity.Property(e => e.UserPaid).HasColumnName("user_paid");

            entity.HasOne(d => d.CustomerNavigation).WithMany(p => p.Bills)
                .HasForeignKey(d => d.Customer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bill_customer");

            entity.HasOne(d => d.StoreNavigation).WithMany(p => p.Bills)
                .HasForeignKey(d => d.Store)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bill_store");

            entity.HasOne(d => d.UserCreatedNavigation).WithMany(p => p.BillUserCreatedNavigations)
                .HasForeignKey(d => d.UserCreated)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bill_user_created");

            entity.HasOne(d => d.UserModifiedNavigation).WithMany(p => p.BillUserModifiedNavigations)
                .HasForeignKey(d => d.UserModified)
                .HasConstraintName("fk_bill_user_modified");
        });

        modelBuilder.Entity<BillDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("bill_detail");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Bill).HasColumnName("bill");
            entity.Property(e => e.Discount)
                .HasPrecision(10, 2)
                .HasColumnName("discount");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'1.00'")
                .HasColumnName("quantity");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
        });

        modelBuilder.Entity<BillDetailView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("bill_detail_view");

            entity.Property(e => e.Ave)
                .HasPrecision(10, 2)
                .HasColumnName("ave");
            entity.Property(e => e.Bill).HasColumnName("bill");
            entity.Property(e => e.Customer)
                .HasDefaultValueSql("'1'")
                .HasColumnName("customer");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Discount)
                .HasPrecision(10, 2)
                .HasColumnName("discount");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("item_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'1.00'")
                .HasColumnName("quantity");
            entity.Property(e => e.Soh)
                .HasPrecision(10, 2)
                .HasColumnName("soh");
            entity.Property(e => e.StockUnit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("stock_unit");
            entity.Property(e => e.StockUnitName)
                .HasMaxLength(50)
                .HasColumnName("stock_unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Value)
                .HasPrecision(21, 4)
                .HasColumnName("value");
        });

        modelBuilder.Entity<BillView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("bill_view");

            entity.Property(e => e.Customer)
                .HasDefaultValueSql("'1'")
                .HasColumnName("customer");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(200)
                .HasColumnName("customer_address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(50)
                .HasColumnName("customer_email")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasColumnName("customer_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(15)
                .HasColumnName("customer_phone");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("datetime")
                .HasColumnName("date_modified");
            entity.Property(e => e.DatePaid)
                .HasColumnType("datetime")
                .HasColumnName("date_paid");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Paid)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("paid");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.TimeCreated)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time_created");
            entity.Property(e => e.TimeModified)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time_modified");
            entity.Property(e => e.TimePaid)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time_paid");
            entity.Property(e => e.TotalValue)
                .HasPrecision(10, 2)
                .HasColumnName("total_value");
            entity.Property(e => e.UserCreated).HasColumnName("user_created");
            entity.Property(e => e.UserModified)
                .HasDefaultValueSql("'1'")
                .HasColumnName("user_modified");
            entity.Property(e => e.UserPaid).HasColumnName("user_paid");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("customer");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<CustomerDebtView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("customer_debt_view");

            entity.Property(e => e.Customer)
                .HasDefaultValueSql("'1'")
                .HasColumnName("customer");
            entity.Property(e => e.Debt)
                .HasPrecision(32, 2)
                .HasColumnName("debt");
        });

        modelBuilder.Entity<CustomerView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("customer_view");

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Debt)
                .HasPrecision(32, 2)
                .HasColumnName("debt");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("item");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.Group)
                .HasDefaultValueSql("'1'")
                .HasColumnName("group");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.SalePrice)
                .HasPrecision(8)
                .HasColumnName("sale_price");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UseLot).HasColumnName("use_lot");
        });

        modelBuilder.Entity<ItemGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("item_group");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsInput).HasColumnName("is_input");
            entity.Property(e => e.IsOutput).HasColumnName("is_output");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<ItemView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("item_view");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.GroupName)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("group_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsInput).HasColumnName("is_input");
            entity.Property(e => e.IsOutput).HasColumnName("is_output");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<LastStoreTransaction>(entity =>
        {
            entity.HasKey(e => new { e.Store, e.Item, e.Unit, e.Lot, e.Year })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0, 0, 0 });

            entity.ToTable("last_store_transaction");

            entity.HasIndex(e => e.LastTransaction, "fk_last_store_transaction_idx");

            entity.HasIndex(e => e.Item, "fk_last_store_transaction_item_idx");

            entity.HasIndex(e => e.Unit, "fk_last_store_transaction_unit_idx");

            entity.Property(e => e.Store).HasColumnName("store");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.Lot)
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("lot");
            entity.Property(e => e.Year)
                .HasDefaultValueSql("'26'")
                .HasColumnName("year");
            entity.Property(e => e.LastTransaction).HasColumnName("last_transaction");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.LastStoreTransactions)
                .HasForeignKey(d => d.Item)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_last_store_transaction_item");

            entity.HasOne(d => d.LastTransactionNavigation).WithMany(p => p.LastStoreTransactions)
                .HasForeignKey(d => d.LastTransaction)
                .HasConstraintName("fk_last_store_transaction");

            entity.HasOne(d => d.StoreNavigation).WithMany(p => p.LastStoreTransactions)
                .HasForeignKey(d => d.Store)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_last_store_transaction_store");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.LastStoreTransactions)
                .HasForeignKey(d => d.Unit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_last_store_transaction_unit");
        });

        modelBuilder.Entity<Param>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PRIMARY");

            entity.ToTable("param");

            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.Value)
                .HasMaxLength(400)
                .HasColumnName("value")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Processing>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("processing");

            entity.HasIndex(e => e.Model, "fk_processing_model_idx");

            entity.HasIndex(e => e.User, "fk_processing_user_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.User)
                .HasDefaultValueSql("'1'")
                .HasColumnName("user");

            entity.HasOne(d => d.ModelNavigation).WithMany(p => p.Processings)
                .HasForeignKey(d => d.Model)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_processing_processing_model");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Processings)
                .HasForeignKey(d => d.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_processing_user");
        });

        modelBuilder.Entity<ProcessingDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("processing_detail");

            entity.HasIndex(e => e.Processing, "fk_processing_idx");

            entity.HasIndex(e => e.Item, "fk_processing_item_idx");

            entity.HasIndex(e => e.Unit, "fk_processing_unit_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsOutput).HasColumnName("is_output");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.Lot)
                .HasMaxLength(10)
                .HasColumnName("lot");
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Processing).HasColumnName("processing");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.Year)
                .HasDefaultValueSql("'26'")
                .HasColumnName("year");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.ProcessingDetails)
                .HasForeignKey(d => d.Item)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_processing_item");

            entity.HasOne(d => d.ProcessingNavigation).WithMany(p => p.ProcessingDetails)
                .HasForeignKey(d => d.Processing)
                .HasConstraintName("fk_processing_detail_processing");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.ProcessingDetails)
                .HasForeignKey(d => d.Unit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_processing_unit");
        });

        modelBuilder.Entity<ProcessingDetailView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("processing_detail_view");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InOut)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValueSql("''")
                .HasColumnName("in_out");
            entity.Property(e => e.IsOutput).HasColumnName("is_output");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.ItemIsInput).HasColumnName("item_is_input");
            entity.Property(e => e.ItemIsOutput).HasColumnName("item_is_output");
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("item_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Lot)
                .HasMaxLength(10)
                .HasColumnName("lot");
            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Processing).HasColumnName("processing");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Soh)
                .HasPrecision(10, 2)
                .HasColumnName("soh");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.StoreName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("store_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'1'")
                .HasColumnName("type");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UseLot).HasColumnName("use_lot");
            entity.Property(e => e.Year)
                .HasDefaultValueSql("'26'")
                .HasColumnName("year");
        });

        modelBuilder.Entity<ProcessingModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("processing_model");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValueSql("''")
                .HasColumnName("description")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'1'")
                .HasColumnName("type");
        });

        modelBuilder.Entity<ProcessingModelDetail>(entity =>
        {
            entity.HasKey(e => new { e.Model, e.Item })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("processing_model_detail");

            entity.HasIndex(e => e.Item, "fk_processing_model_idx");

            entity.HasIndex(e => e.Unit, "fk_processing_model_unit_idx");

            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.IsOutput).HasColumnName("is_output");
            entity.Property(e => e.LostPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'10.00'")
                .HasColumnName("lost_percent");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'1.00'")
                .HasColumnName("quantity");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.ProcessingModelDetails)
                .HasForeignKey(d => d.Item)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_processing_model");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.ProcessingModelDetails)
                .HasForeignKey(d => d.Unit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_processing_model_unit");
        });

        modelBuilder.Entity<ProcessingModelDetailView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("processing_model_detail_view");

            entity.Property(e => e.InOut)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValueSql("''")
                .HasColumnName("in_out");
            entity.Property(e => e.IsOutput).HasColumnName("is_output");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.ItemIsInput).HasColumnName("item_is_input");
            entity.Property(e => e.ItemIsOutput).HasColumnName("item_is_output");
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("item_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.LostPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("'10.00'")
                .HasColumnName("lost_percent");
            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'1.00'")
                .HasColumnName("quantity");
            entity.Property(e => e.Soh)
                .HasPrecision(10, 2)
                .HasColumnName("soh");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.StoreName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("store_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UseLot).HasColumnName("use_lot");
        });

        modelBuilder.Entity<ProcessingType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("processing_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<ProfitView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("profit_view");

            entity.Property(e => e.Ave)
                .HasPrecision(10, 2)
                .HasColumnName("ave");
            entity.Property(e => e.BillPrice)
                .HasPrecision(11, 2)
                .HasColumnName("bill_price");
            entity.Property(e => e.Profit)
                .HasPrecision(22, 4)
                .HasColumnName("profit");
        });

        modelBuilder.Entity<Receiving>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("receiving");

            entity.HasIndex(e => e.Store, "fk_receiving_store_idx");

            entity.HasIndex(e => e.User, "fk_receiving_user_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Store)
                .HasDefaultValueSql("'1'")
                .HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.StoreNavigation).WithMany(p => p.Receivings)
                .HasForeignKey(d => d.Store)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receiving_store");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Receivings)
                .HasForeignKey(d => d.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receiving_user");
        });

        modelBuilder.Entity<ReceivingDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("receiving_detail");

            entity.HasIndex(e => e.Receiving, "fk_receiving_idx");

            entity.HasIndex(e => e.Item, "fk_receiving_item_idx");

            entity.HasIndex(e => e.Unit, "fk_receiving_unit_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Price)
                .HasPrecision(10)
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasPrecision(8, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Receiving).HasColumnName("receiving");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.ReceivingDetails)
                .HasForeignKey(d => d.Item)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receiving_item");

            entity.HasOne(d => d.ReceivingNavigation).WithMany(p => p.ReceivingDetails)
                .HasForeignKey(d => d.Receiving)
                .HasConstraintName("fk_receiving");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.ReceivingDetails)
                .HasForeignKey(d => d.Unit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_receiving_unit");
        });

        modelBuilder.Entity<ReceivingDetailView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("receiving_detail_view");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("item_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Price)
                .HasPrecision(10)
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasPrecision(8, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Receiving).HasColumnName("receiving");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Value)
                .HasPrecision(18, 2)
                .HasColumnName("value");
        });

        modelBuilder.Entity<StockCardView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("stock_card_view");

            entity.Property(e => e.Ave)
                .HasPrecision(10, 2)
                .HasColumnName("ave");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.DetailId).HasColumnName("detail_id");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.Lot)
                .HasMaxLength(10)
                .HasColumnName("lot");
            entity.Property(e => e.LotAve)
                .HasPrecision(10, 2)
                .HasColumnName("lot_ave");
            entity.Property(e => e.LotSoh)
                .HasPrecision(10, 2)
                .HasColumnName("lot_soh");
            entity.Property(e => e.LotValue)
                .HasPrecision(10, 2)
                .HasColumnName("lot_value");
            entity.Property(e => e.MainId).HasColumnName("main_id");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Soh)
                .HasPrecision(10, 2)
                .HasColumnName("soh");
            entity.Property(e => e.Store).HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'1'")
                .HasColumnName("type");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.User)
                .HasDefaultValueSql("'1'")
                .HasColumnName("user");
            entity.Property(e => e.Value)
                .HasPrecision(10, 2)
                .HasColumnName("value");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<StockView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("stock_view");

            entity.Property(e => e.Ave)
                .HasPrecision(10, 2)
                .HasColumnName("ave");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.GroupName)
                .HasMaxLength(51)
                .HasColumnName("group_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.ItemGroup)
                .HasDefaultValueSql("'1'")
                .HasColumnName("item_group");
            entity.Property(e => e.ItemIsInput).HasColumnName("item_is_input");
            entity.Property(e => e.ItemIsOutput).HasColumnName("item_is_output");
            entity.Property(e => e.ItemName)
                .HasMaxLength(121)
                .HasColumnName("item_name");
            entity.Property(e => e.LastTransaction).HasColumnName("last_transaction");
            entity.Property(e => e.Lot)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("lot");
            entity.Property(e => e.LotValue)
                .HasMaxLength(8)
                .HasColumnName("lot_value");
            entity.Property(e => e.NoSlotValue)
                .HasPrecision(10, 2)
                .HasColumnName("no_slot_value");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Soh)
                .HasPrecision(10, 2)
                .HasColumnName("soh");
            entity.Property(e => e.Store).HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.UnitName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("unit_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Value)
                .HasPrecision(10, 2)
                .HasColumnName("value");
            entity.Property(e => e.Year)
                .HasDefaultValueSql("'26'")
                .HasColumnName("year");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("store");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<StoreTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("store_transaction");

            entity.HasIndex(e => e.Item, "fk_store_transaction_item_idx");

            entity.HasIndex(e => e.Store, "fk_store_transaction_store_idx");

            entity.HasIndex(e => e.Unit, "fk_store_transaction_unit_idx");

            entity.HasIndex(e => e.User, "fk_store_transaction_user_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Ave)
                .HasPrecision(10, 2)
                .HasColumnName("ave");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DetailId).HasColumnName("detail_id");
            entity.Property(e => e.Item).HasColumnName("item");
            entity.Property(e => e.Lot)
                .HasMaxLength(10)
                .HasColumnName("lot");
            entity.Property(e => e.LotAve)
                .HasPrecision(10, 2)
                .HasColumnName("lot_ave");
            entity.Property(e => e.LotSoh)
                .HasPrecision(10, 2)
                .HasColumnName("lot_soh");
            entity.Property(e => e.LotValue)
                .HasPrecision(10, 2)
                .HasColumnName("lot_value");
            entity.Property(e => e.MainId).HasColumnName("main_id");
            entity.Property(e => e.Ordinal)
                .HasDefaultValueSql("'1'")
                .HasColumnName("ordinal");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 2)
                .HasColumnName("quantity");
            entity.Property(e => e.Soh)
                .HasPrecision(10, 2)
                .HasColumnName("soh");
            entity.Property(e => e.Store).HasColumnName("store");
            entity.Property(e => e.Time)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("time");
            entity.Property(e => e.Type)
                .HasDefaultValueSql("'1'")
                .HasColumnName("type");
            entity.Property(e => e.Unit)
                .HasDefaultValueSql("'1'")
                .HasColumnName("unit");
            entity.Property(e => e.User)
                .HasDefaultValueSql("'1'")
                .HasColumnName("user");
            entity.Property(e => e.Value)
                .HasPrecision(10, 2)
                .HasColumnName("value");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.ItemNavigation).WithMany(p => p.StoreTransactions)
                .HasForeignKey(d => d.Item)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_store_transaction_item");

            entity.HasOne(d => d.StoreNavigation).WithMany(p => p.StoreTransactions)
                .HasForeignKey(d => d.Store)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_store_transaction_store");

            entity.HasOne(d => d.UnitNavigation).WithMany(p => p.StoreTransactions)
                .HasForeignKey(d => d.Unit)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_store_transaction_unit");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.StoreTransactions)
                .HasForeignKey(d => d.User)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_store_transaction_user");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("unit");

            entity.HasIndex(e => e.Name, "name_UNIQUE").IsUnique();

            entity.HasIndex(e => e.FullName, "sort_name_UNIQUE").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BaseUnit).HasColumnName("base_unit");
            entity.Property(e => e.BuExchange)
                .HasDefaultValueSql("'1'")
                .HasColumnName("bu_exchange");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("full_name");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "username_UNIQUE").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IsSuper).HasColumnName("is_super");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("password");
            entity.Property(e => e.Token)
                .HasMaxLength(1000)
                .HasColumnName("token");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
