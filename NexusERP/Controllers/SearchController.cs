using Microsoft.AspNetCore.Mvc;
using NexusERP.DTOs;
using NexusERP.Models;
using NexusERP.Repositories;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    public class SearchController : Controller
    {
        private SearchRepository repo;

        public SearchController(SearchRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> GlobalSearch(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new List<SearchResultDto>()); 
            }

            string query = q.ToLower().Trim();

            List<SearchResultDto> resultados = new List<SearchResultDto>();

            List<Empleado> empleados = await this.repo.SearchEmpleadosAsync(query);

            foreach (var emp in empleados)
            {
                resultados.Add(new SearchResultDto
                {
                    Categoria = "Empleado",
                    Titulo = $"{emp.Nombre} {emp.Apellidos}",
                    Subtitulo = $"DNI: {emp.Dni}",
                    Url = Url.Action("Details", "Empleados", new { id = emp.Id }),
                    Icono = "persona"
                });
            }

            List<Cliente> clientes = await this.repo.SearchClientesAsync(query);

            foreach (var cli in clientes)
            {
                resultados.Add(new SearchResultDto
                {
                    Categoria = "Cliente",
                    Titulo = cli.RazonSocial,
                    Subtitulo = $"CIF/NIF: {cli.CifNif}",
                    Url = Url.Action("Details", "Clientes", new { id = cli.Id }),
                    Icono = "cliente"
                });
            }

            List<Factura> facturas = await this.repo.SearchFacturasAsync(query);

            foreach (var fac in facturas)
            {
                resultados.Add(new SearchResultDto
                {
                    Categoria = "Factura",
                    Titulo = fac.NumeroFactura,
                    Subtitulo = $"Total: {fac.TotalFactura.ToString("N2")} €",
                    Url = Url.Action("Details", "Facturacion", new { id = fac.Id }),
                    Icono = "factura"
                });
            }

            return Json(resultados.OrderBy(r => r.Categoria));
        }
    }
}
