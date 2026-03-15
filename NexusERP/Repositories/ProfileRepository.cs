using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Models;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class ProfileRepository
    {

        private NexusContext context;

        public ProfileRepository(NexusContext context)
        {
            this.context = context;
        }

        public async Task<Usuario> GetPerfilUsuarioAsync(int idUsuario)
        {
            return await this.context.Usuarios
                .Include(u => u.Empleado)
                    .ThenInclude(u => u.Departamento)
                .FirstOrDefaultAsync(u => u.Id == idUsuario);
        }

        public async Task<bool> UpdatePerfilUsuarioAsync(int idUsuario, string nombre, string email)
        {
            Usuario user = await this.GetUsuarioAsync(idUsuario);
            if (user == null) return false;

            bool emailExiste = await this.context.Usuarios
                .AnyAsync(u => u.Email == email && u.Id != idUsuario);

            if (emailExiste) return false;

            user.Nombre = nombre;
            user.Email = email;
            await this.context.SaveChangesAsync();
            return true;
        }
        public async Task<Usuario> GetUsuarioAsync(int idUsuario)
        {
            return await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == idUsuario);
        }

    }
}
