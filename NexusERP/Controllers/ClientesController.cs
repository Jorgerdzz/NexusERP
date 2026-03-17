using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Helpers;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.Services;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
    public class ClientesController : Controller
    {
        private ClienteRepository repo;
        private HelperSessionContextAccessor contextAccessor;

        public ClientesController(ClienteRepository repo, HelperSessionContextAccessor contextAccessor)
        {
            this.repo = repo;
            this.contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            List<Cliente> clientes = await this.repo.GetClientesAsync();
            return View(clientes);
        }

        public async Task<IActionResult> Details(int id)
        {
            Cliente cliente = await this.repo.GetClienteAsync(id);
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string razonSocial, string cifNif, string email)
        {
            int empresaId = this.contextAccessor.GetEmpresaIdSession();

            Cliente nuevo = new Cliente
            {
                EmpresaId = empresaId,
                RazonSocial = razonSocial,
                CifNif = cifNif,
                Email = email,
                Activo = true
            };

            bool creado = await this.repo.CrearClienteAsync(nuevo);

            if (creado)
            {
                AlertService.Toast(TempData, "Cliente guardado correctamente", "success");
            }
            else
            {
                AlertService.Error(TempData, "Hubo un error al guardar el cliente");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string razonSocial, string cifNif, string email)
        {
            int empresaId = this.contextAccessor.GetEmpresaIdSession();

            Cliente nuevo = new Cliente
            {
                EmpresaId = empresaId,
                RazonSocial = razonSocial,
                CifNif = cifNif,
                Email = email,
                Activo = true
            };

            bool actualizado = await this.repo.UpdateClienteAsync(nuevo);

            if (actualizado)
            {
                AlertService.Toast(TempData, "Cliente actualizado correctamente", "success");
            }
            else
            {
                AlertService.Error(TempData, "Hubo un error al actualizar el cliente");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            bool eliminado = await this.repo.DeleteClienteAsync(id);

            if (eliminado)
            {
                AlertService.Toast(TempData, "Cliente eliminado correctamente", "success");
            }
            else
            {
                AlertService.Error(TempData, "Hubo un error al eliminar el cliente");
            }

            return RedirectToAction("Index");
        }

    }
}
