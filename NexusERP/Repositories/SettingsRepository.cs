using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Models;
using System.Threading.Tasks;

namespace NexusERP.Repositories
{
    public class SettingsRepository
    {
        private NexusContext context;

        public SettingsRepository(NexusContext context)
        {
            this.context = context;
        }

        public async Task<Empresa> GetEmpresaAsync(int idEmpresa)
        {
            return await this.context.Empresas
                .FirstOrDefaultAsync(e => e.Id == idEmpresa);
        }

        public async Task<bool> UpdateEmpresaAsync(int idEmpresa, string nombreComercial, string razonSocial, string cif)
        {
            Empresa empresa = await this.GetEmpresaAsync(idEmpresa);
            if (empresa == null) return false;
            bool cifExiste = await this.context.Empresas.AnyAsync(e => e.Cif == cif && e.Id != idEmpresa);
            if (cifExiste) return false;
            empresa.NombreComercial = nombreComercial;
            empresa.RazonSocial = razonSocial;
            empresa.Cif = cif;
            await this.context.SaveChangesAsync();
            return true;
        }

    }
}
