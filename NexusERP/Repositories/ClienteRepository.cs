using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Models;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class ClienteRepository
    {
        private NexusContext context;

        public ClienteRepository(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Cliente>> GetClientesAsync()
        {
            return await this.context.Clientes.OrderBy(c => c.RazonSocial).ToListAsync();
        }

        public async Task<Cliente> GetClienteAsync(int idCliente)
        {
            return await this.context.Clientes.Where(c => c.Id == idCliente).FirstOrDefaultAsync();
        }

        public async Task<bool> CrearClienteAsync(Cliente cliente)
        {
            try
            {
                bool existe = await this.context.Clientes.AnyAsync(c => c.CifNif == cliente.CifNif && c.EmpresaId == cliente.EmpresaId);
                if (existe) return false;
                await this.context.Clientes.AddAsync(cliente);
                await this.context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateClienteAsync(Cliente cliente)
        {
            try
            {
                Cliente c = await this.GetClienteAsync(cliente.Id);

                if (c == null) return false;

                c.RazonSocial = cliente.RazonSocial;
                c.CifNif = cliente.CifNif;
                c.Email = cliente.Email;
                c.Activo = cliente.Activo;

                await this.context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteClienteAsync(int idCliente)
        {
            try
            {
                Cliente c = await this.GetClienteAsync(idCliente);
                if (c == null) return false;
                this.context.Clientes.Remove(c);
                await this.context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
