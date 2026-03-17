using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Models;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class SearchRepository
    {
        private NexusContext context;

        public SearchRepository(NexusContext context)
        {
            this.context = context;
        }

        public async Task<List<Empleado>> SearchEmpleadosAsync(string query)
        {
            return await this.context.Empleados
                .Where(e => e.Nombre.ToLower().Contains(query) ||
                            e.Apellidos.ToLower().Contains(query) ||
                            e.Dni.ToLower().Contains(query))
                .Take(3)
                .ToListAsync();
        }

        public async Task<List<Cliente>> SearchClientesAsync(string query)
        {
            return await this.context.Clientes
                .Where(c => c.RazonSocial.ToLower().Contains(query) ||
                            c.CifNif.ToLower().Contains(query))
                .Take(3)
                .ToListAsync();
        }

        public async Task<List<Factura>> SearchFacturasAsync(string query)
        {
            return await this.context.Facturas
                .Where(f => f.NumeroFactura.ToLower().Contains(query))
                .Take(3)
                .ToListAsync();
        }

    }
}
