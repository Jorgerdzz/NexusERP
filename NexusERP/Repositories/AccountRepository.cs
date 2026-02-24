using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Enums;
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

        public async Task<(bool exito, string mensaje, Usuario? usuarioCreado)> RegistrarCuentaAsync(RegistroViewModel model)
        {
            bool emailExiste = await this.context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (emailExiste) return (false, "Email ya registrado en el sistema", null);

            bool cifExiste = await this.context.Empresas.AnyAsync(e => e.Cif == model.CIF);
            if (emailExiste) return (false, "Ya existe una empresa registrada con ese CIF", null);

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

                this.context.Add(empresa);
                await this.context.SaveChangesAsync();

                Usuario user = new Usuario
                {
                    
                    EmpresaId = empresa.Id,

                    Nombre = model.NombreUsuario,
                    Email = model.Email,
                    PasswordHash = HashearPassword(model.Password),
                    Rol = (int)RolesUsuario.Admin,
                    Activo = true
                };

                this.context.Usuarios.Add(user);
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

        private string HashearPassword(string password)
        {
            using(SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public async Task<(bool acceso, string mensaje, Usuario? user)> ComprobarUsuarioAsync(LoginViewModel model)
        {
            var consulta = from datos in this.context.Usuarios
                       where datos.Email == model.Email
                       select datos;
            Usuario user = await consulta.FirstOrDefaultAsync();

            if(user == null)
            {
                return (false, "El correo electrónico no está registrado.", null);
            }

            if(user.PasswordHash == this.HashearPassword(model.Password))
            {
                return (true, "Usuario logueado correctamente", user);
            }
            else
            {
                return (false, "La contraseña es incorrecta.", null);
            }
            
        }

    }
}
