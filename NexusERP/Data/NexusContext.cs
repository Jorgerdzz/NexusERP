using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.ViewModels;

namespace NexusERP.Data;

public partial class NexusContext : DbContext
{
    private readonly HelperSessionContextAccessor contextAccessor;
    public NexusContext()
    {
    }

    public NexusContext(DbContextOptions<NexusContext> options, HelperSessionContextAccessor contextAccessor)
        : base(options)
    {
        this.contextAccessor = contextAccessor;
    }

    public virtual DbSet<ApuntesContable> ApuntesContables { get; set; }

    public virtual DbSet<AsientosContable> AsientosContables { get; set; }

    public virtual DbSet<ConceptosFijosEmpleado> ConceptosFijosEmpleados { get; set; }

    public virtual DbSet<ConceptosSalariale> ConceptosSalariales { get; set; }

    public virtual DbSet<ControlGasto> ControlGastos { get; set; }

    public virtual DbSet<CuentasContable> CuentasContables { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Nomina> Nominas { get; set; }

    public virtual DbSet<NominaDetalle> NominaDetalles { get; set; }

    public virtual DbSet<SeguridadUsuario> SeguridadUsuarios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\DEVELOPER;Database=ERP;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApuntesContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ApuntesC__3214EC07DDE98A5C");

            entity.Property(e => e.Debe)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Haber)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Asiento).WithMany(p => p.ApuntesContables)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__ApuntesCo__Asien__7D439ABD");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.ApuntesContables)
                .HasForeignKey(d => d.CuentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ApuntesCo__Cuent__7E37BEF6");
        });

        modelBuilder.Entity<AsientosContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asientos__3214EC07F7C10C18");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Glosa).HasMaxLength(255);

            entity.HasOne(d => d.Empresa).WithMany(p => p.AsientosContables)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AsientosC__Empre__52593CB8");
        });

        modelBuilder.Entity<ConceptosFijosEmpleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Concepto__3214EC0763BEA5AB");

            entity.ToTable("ConceptosFijosEmpleado");

            entity.HasIndex(e => new { e.EmpleadoId, e.ConceptoId }, "UQ_Empleado_Concepto").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.ImporteFijo).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Concepto).WithMany(p => p.ConceptosFijosEmpleados)
                .HasForeignKey(d => d.ConceptoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conceptos__Conce__0C85DE4D");

            entity.HasOne(d => d.Empleado).WithMany(p => p.ConceptosFijosEmpleados)
                .HasForeignKey(d => d.EmpleadoId)
                .HasConstraintName("FK__Conceptos__Emple__0B91BA14");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ConceptosFijosEmpleados)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Conceptos__Empre__0A9D95DB");
        });

        modelBuilder.Entity<ConceptosSalariale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Concepto__3214EC078876DB4F");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.TributaIrpf)
                .HasDefaultValue(true)
                .HasColumnName("TributaIRPF");

            entity.HasOne(d => d.CuentaContable).WithMany(p => p.ConceptosSalariales)
                .HasForeignKey(d => d.CuentaContableId)
                .HasConstraintName("FK_Conceptos_Cuenta");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ConceptosSalariales)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Conceptos_Empresa");
        });

        modelBuilder.Entity<ControlGasto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ControlG__3214EC07DAF744C9");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImporteGasto).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Departamento).WithMany(p => p.ControlGastos)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ControlGa__Depar__72C60C4A");

            entity.HasOne(d => d.Empleado).WithMany(p => p.ControlGastos)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ControlGa__Emple__73BA3083");

            entity.HasOne(d => d.Empresa).WithMany(p => p.ControlGastos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ControlGa__Empre__71D1E811");

            entity.HasOne(d => d.Nomina).WithMany(p => p.ControlGastos)
                .HasForeignKey(d => d.NominaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ControlGa__Nomin__74AE54BC");
        });

        modelBuilder.Entity<CuentasContable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CuentasC__3214EC07BD38B4CB");

            entity.HasIndex(e => new { e.EmpresaId, e.Codigo }, "UQ_Cuenta_Empresa").IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Tipo).HasMaxLength(50);

            entity.HasOne(d => d.Empresa).WithMany(p => p.CuentasContables)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CuentasCo__Empre__4E88ABD4");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departam__3214EC0737BC0F05");

            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.PresupuestoMensual)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Departamentos)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Departame__Empre__3D5E1FD2");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empleado__3214EC0709DD15A0");

            entity.HasIndex(e => new { e.EmpresaId, e.Dni }, "UQ_DNI_Empresa").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Apellidos).HasMaxLength(150);
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DNI");
            entity.Property(e => e.EmailCorporativo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FotoUrl).HasMaxLength(500);
            entity.Property(e => e.Iban)
                .HasMaxLength(34)
                .IsUnicode(false)
                .HasColumnName("IBAN");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.NumSeguridadSocial)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NumeroHijos).HasDefaultValue(0);
            entity.Property(e => e.PorcentajeDiscapacidad).HasDefaultValue(0);
            entity.Property(e => e.SalarioBrutoAnual).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Departamento).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.DepartamentoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Empleados__Depar__44FF419A");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Empleados__Empre__440B1D61");
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empresas__3214EC079445C365");

            entity.HasIndex(e => e.Cif, "UQ__Empresas__C1F8DC5E997B949B").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Cif)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CIF");
            entity.Property(e => e.FechaAlta)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NombreComercial).HasMaxLength(150);
            entity.Property(e => e.RazonSocial).HasMaxLength(150);
        });

        modelBuilder.Entity<Nomina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Nominas__3214EC07D03EC1D8");

            entity.Property(e => e.BaseCotizacionCc)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("BaseCotizacion_CC");
            entity.Property(e => e.BaseCotizacionCp)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("BaseCotizacion_CP");
            entity.Property(e => e.BaseIrpf)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("BaseIRPF");
            entity.Property(e => e.CosteTotalEmpresa)
                .HasComputedColumnSql("([TotalDevengado]+[SS_Empresa_Total])", false)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LiquidoApercibir)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("LiquidoAPercibir");
            entity.Property(e => e.PorcentajeIrpf)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("PorcentajeIRPF");
            entity.Property(e => e.SsEmpresaAccidentesTrabajo)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SS_Empresa_AccidentesTrabajo");
            entity.Property(e => e.SsEmpresaContingenciasComunes)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SS_Empresa_ContingenciasComunes");
            entity.Property(e => e.SsEmpresaDesempleo)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SS_Empresa_Desempleo");
            entity.Property(e => e.SsEmpresaFogasa)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SS_Empresa_Fogasa");
            entity.Property(e => e.SsEmpresaFormacion)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SS_Empresa_Formacion");
            entity.Property(e => e.SsEmpresaTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SS_Empresa_Total");
            entity.Property(e => e.TotalDeducciones).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDevengado).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Asiento).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.AsientoId)
                .HasConstraintName("FK__Nominas__Asiento__6A30C649");

            entity.HasOne(d => d.Empleado).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.EmpleadoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nominas__Emplead__693CA210");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Nominas)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Nominas__Empresa__68487DD7");
        });

        modelBuilder.Entity<NominaDetalle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NominaDe__3214EC07C32A2A64");

            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.ConceptoNombre).HasMaxLength(150);
            entity.Property(e => e.Importe).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Concepto).WithMany(p => p.NominaDetalles)
                .HasForeignKey(d => d.ConceptoId)
                .HasConstraintName("FK__NominaDet__Conce__787EE5A0");

            entity.HasOne(d => d.Nomina).WithMany(p => p.NominaDetalles)
                .HasForeignKey(d => d.NominaId)
                .HasConstraintName("FK__NominaDet__Nomin__778AC167");
        });

        modelBuilder.Entity<SeguridadUsuario>(entity =>
        {
            entity.HasKey(e => e.IdSeguridad).HasName("PK__Segurida__D7A19C259EB43EA6");

            entity.ToTable("SeguridadUsuario");

            entity.Property(e => e.Salt).HasMaxLength(50);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.SeguridadUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Id_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC075CB23A85");

            entity.HasIndex(e => e.Email, "IX_Usuarios_Email");

            entity.HasIndex(e => e.Email, "UQ_Usuario_Email").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Nombre).HasMaxLength(100);

            entity.HasOne(d => d.Empleado).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EmpleadoId)
                .HasConstraintName("FK_Usuario_Empleado");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__Empres__49C3F6B7");
        });

        modelBuilder.Entity<Empleado>().HasQueryFilter(e => e.EmpresaId == this.contextAccessor.GetEmpresaIdSession());
        modelBuilder.Entity<Departamento>().HasQueryFilter(d => d.EmpresaId == this.contextAccessor.GetEmpresaIdSession());
        modelBuilder.Entity<Nomina>().HasQueryFilter(n => n.EmpresaId == this.contextAccessor.GetEmpresaIdSession());
        modelBuilder.Entity<AsientosContable>().HasQueryFilter(a => a.EmpresaId == this.contextAccessor.GetEmpresaIdSession());
        modelBuilder.Entity<ControlGasto>().HasQueryFilter(c => c.EmpresaId == this.contextAccessor.GetEmpresaIdSession());
        modelBuilder.Entity<CuentasContable>().HasQueryFilter(c => c.EmpresaId == this.contextAccessor.GetEmpresaIdSession());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
