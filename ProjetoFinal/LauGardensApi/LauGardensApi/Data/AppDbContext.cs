using System;
using System.Collections.Generic;
using LauGardensApi.Data.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace LauGardensApi.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Funcionario> Funcionarios { get; set; }

    public virtual DbSet<Planta> Plantas { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Utilizador> Utilizadores { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;port=3306;database=LausGarden;user=root;password=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.3.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.UtilizadorId, "UtilizadorId").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Funcao).HasMaxLength(50);
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.Telefone).HasMaxLength(20);

            entity.HasOne(d => d.Utilizador).WithOne(p => p.Funcionario)
                .HasForeignKey<Funcionario>(d => d.UtilizadorId)
                .HasConstraintName("fk_func_utilizador");
        });

        modelBuilder.Entity<Planta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => new { e.Nome, e.Categoria }, "Nome").IsUnique();

            entity.Property(e => e.Categoria).HasMaxLength(50);
            entity.Property(e => e.Descricao).HasColumnType("text");
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.Preco).HasPrecision(10, 2);
            entity.Property(e => e.UrlImagem).HasMaxLength(255);
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Stock");

            entity.HasIndex(e => e.PlantaId, "PlantaId").IsUnique();

            entity.HasIndex(e => e.AtualizadoPorFuncionarioId, "fk_stock_func");

            entity.Property(e => e.UltimaAtualizacao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");

            entity.HasOne(d => d.AtualizadoPorFuncionario).WithMany(p => p.Stocks)
                .HasForeignKey(d => d.AtualizadoPorFuncionarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_stock_func");

            entity.HasOne(d => d.Planta).WithOne(p => p.Stock)
                .HasForeignKey<Stock>(d => d.PlantaId)
                .HasConstraintName("fk_stock_planta");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.NomeUtilizador, "NomeUtilizador").IsUnique();

            entity.Property(e => e.NomeUtilizador).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'colaborador'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
