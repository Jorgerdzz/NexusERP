using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Filters;
using NexusERP.Models;
using NexusERP.Repositories;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    [AuthorizeUser(Rol = RolesUsuario.Admin)]
    public class DepartamentsController : Controller
    {
        private DepartamentsRepository repo;

        public DepartamentsController(DepartamentsRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            List<Departamento> departamentos = await this.repo.GetDepartamentosAsync();
            return View(departamentos);
        }
    }
}
