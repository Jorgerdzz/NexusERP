using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Enums;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.ViewModels;

namespace NexusERP.Repositories
{
    public class EmpleadosRepository
    {
        private NexusContext context;
        private HelperSessionContextAccessor contextAccessor;

        public EmpleadosRepository(NexusContext context, HelperSessionContextAccessor contextAccessor)
        {
            this.context = context;
            this.contextAccessor = contextAccessor;
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            return await this.context.Empleados.ToListAsync();
        }

        public async Task<List<Empleado>> GetEmpleadosDepartamentoAsync(int? idDepartamento)
        {
            var consulta = from datos in this.context.Empleados
                           where datos.DepartamentoId == idDepartamento
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<int> GetTotalEmpleadosAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<decimal> GetSalarioPromedioAnualAsync()
        {
            return await this.context.Empleados.AverageAsync(e => (decimal?)e.SalarioBrutoAnual) ?? 0;
        }

        public async Task<bool> CreateEmpleadoAsync(Empleado empleado)
        {
            using var transaction = await this.context.Database.BeginTransactionAsync();
            try
            {
                empleado.EmpresaId = this.contextAccessor.GetEmpresaIdSession();
                await this.context.Empleados.AddAsync(empleado);
                await this.context.SaveChangesAsync();

                string passwordPorDefecto = "1234";

                Usuario user = new Usuario
                {
                    EmpresaId = empleado.EmpresaId,
                    Nombre = empleado.Nombre + " " + empleado.Apellidos,
                    Email = empleado.EmailCorporativo,
                    Rol = (int)RolesUsuario.Empleado,
                    EmpleadoId = empleado.Id,
                    Activo = true,
                    Password = passwordPorDefecto
                };

                await this.context.Usuarios.AddAsync(user);
                await this.context.SaveChangesAsync();

                SeguridadUsuario userSecurity = new SeguridadUsuario
                {
                    IdUsuario = user.Id,
                    Salt = HelperTools.GenerateSalt(),
                };

                userSecurity.PasswordHash = HelperCryptography.EncryptPassword(passwordPorDefecto, userSecurity.Salt);

                await this.context.SeguridadUsuarios.AddAsync(userSecurity);
                await this.context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // ¡EL VERDADERO ERROR DE SQL SERVER ESTÁ AQUÍ!
                string errorRealDeBaseDeDatos = ex.InnerException != null ? ex.InnerException.Message : "No hay detalle interno";

                // Pon un punto de interrupción (BreakPoint) en la línea del return false;
                return false;
            }
        }

    }
}
