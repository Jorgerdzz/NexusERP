using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Enums;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace NexusERP.Repositories
{
    public class AccountRepository
    {
        private NexusContext context;

        public AccountRepository(NexusContext context)
        {
            this.context = context;
        }

        public async Task<(bool exito, string mensaje, Usuario? usuarioCreado)> RegisterUserAsync(RegistroViewModel model)
        {
            bool emailExiste = await this.context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (emailExiste) return (false, "Email ya registrado en el sistema", null);

            bool cifExiste = await this.context.Empresas.AnyAsync(e => e.Cif == model.CIF);
            if (cifExiste) return (false, "Ya existe una empresa registrada con ese CIF", null);

            using var transaction = await this.context.Database.BeginTransactionAsync();

            try
            {
                Empresa empresa = new Empresa
                {
                    NombreComercial = model.NombreEmpresa,
                    RazonSocial = model.NombreEmpresa,
                    Cif = model.CIF,
                    FechaAlta = DateTime.Now,
                    Activo = true
                };

                await this.context.Empresas.AddAsync(empresa);
                await this.context.SaveChangesAsync();

                Usuario user = new Usuario
                {
                    EmpresaId = empresa.Id,
                    Nombre = model.NombreUsuario,
                    Email = model.Email,
                    Rol = (int)RolesUsuario.Admin,
                    Activo = true,
                    Password = model.Password
                };

                await this.context.Usuarios.AddAsync(user);
                await this.context.SaveChangesAsync();

                SeguridadUsuario userSecurity = new SeguridadUsuario
                {
                    IdUsuario = user.Id,
                    Salt = HelperTools.GenerateSalt(),
                    PasswordHash = HelperCryptography.EncryptPassword(model.Password, HelperTools.GenerateSalt())
                };

                userSecurity.Salt = HelperTools.GenerateSalt();
                userSecurity.PasswordHash = HelperCryptography.EncryptPassword(model.Password, userSecurity.Salt);

                await this.context.SeguridadUsuarios.AddAsync(userSecurity);
                await this.context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, "Cuenta creada con éxito", user);

            }
            catch
            {
                await transaction.RollbackAsync();
                return (false, "Error al crear la cuenta", null);
            }
        }

        public async Task<(bool acceso, string mensaje, Usuario? user)> LogInUserAsync(LoginViewModel model)
        {
            var datosLogin = await (from u in this.context.Usuarios
                                    join s in this.context.SeguridadUsuarios on u.Id equals s.IdUsuario
                                    join e in this.context.Empresas on u.EmpresaId equals e.Id
                                    where u.Email == model.Email
                                    select new { Usuario = u, Seguridad = s, Empresa = e })
                                    .FirstOrDefaultAsync();

            if(datosLogin == null)
            {
                return (false, "El correo electrónico no está registrado.", null);
            }
            else
            {
                string saltBD = datosLogin.Seguridad.Salt;
                byte[] passwordHashBD = datosLogin.Seguridad.PasswordHash;

                byte[] passwordGenerado = HelperCryptography.EncryptPassword(model.Password, saltBD);

                bool esValido = HelperTools.CompareArrays(passwordGenerado, passwordHashBD);

                if (esValido)
                {
                    return (true, "Usuario logueado correctamente", datosLogin.Usuario);
                }
                else
                {
                    return (false, "La contraseña es incorrecta.", null);
                }
            }
            
        }

    }
}
