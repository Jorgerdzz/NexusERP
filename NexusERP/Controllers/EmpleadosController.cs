using Microsoft.AspNetCore.Mvc;
using NexusERP.Enums;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace NexusERP.Controllers
{
    public class EmpleadosController : Controller
    {
        private EmpleadosRepository repoEmpleados;
        private DepartamentsRepository repoDepartamentos;

        public EmpleadosController(EmpleadosRepository repoEmpleados, DepartamentsRepository repoDepartamentos)
        {
            this.repoEmpleados = repoEmpleados;
            this.repoDepartamentos = repoDepartamentos;
        }

        public async Task<IActionResult> Index(int? idDepartamento)
        {
            EmpleadosViewModel model = new EmpleadosViewModel();
            model.Departamentos = await this.repoDepartamentos.GetDepartamentosAsync();
            if (idDepartamento != null)
            {
                model.Empleados = await this.repoEmpleados.GetEmpleadosDepartamentoAsync(idDepartamento);
                ViewBag.DepartamentoSeleccionado = idDepartamento.Value;
            }
            else
            {
                model.Empleados = await this.repoEmpleados.GetEmpleadosAsync();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmpleadoViewModel model)
        {
            if (ModelState.IsValid)
            {
                Empleado emp = new Empleado
                {
                    DepartamentoId = model.DepartamentoId,
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    Dni = model.DNI,
                    EmailCorporativo = model.EmailCorporativo,
                    Telefono = model.Telefono,
                    FechaNacimiento = model.FechaNacimiento,
                    NumSeguridadSocial = model.NumSeguridadSocial,
                    FechaAntiguedad = model.FechaAntiguedad,
                    GrupoCotizacion = (int)model.GrupoCotizacion,
                    EstadoCivil = (int)model.EstadoCivil,
                    SalarioBrutoAnual = model.SalarioBrutoAnual,
                    Iban = model.IBAN?.Replace(" ", "").ToUpper(),
                    NumeroHijos = model.NumeroHijos,
                    PorcentajeDiscapacidad = model.PorcentajeDiscapacidad,
                    Activo = (model.Activo == EstadoEmpleado.Activo)
                };

                bool exito = await this.repoEmpleados.CreateEmpleadoAsync(emp);

                if (exito)
                {
                    TempData["SuccessMessage"] = "Empleado añadido correctamente a la plantilla.";
                    return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado al guardar en la base de datos.";
                }

            }
            else
            {
                TempData["ErrorMessage"] = "Revisa los datos del formulario. Algunos campos no tienen el formato correcto.";
                return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
            }
            return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
        }

        [HttpPost]
        public async Task<IActionResult> ImportCsv(IFormFile file, int departamentoId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Debes seleccionar un archivo CSV.";
                return RedirectToAction("Details", "Departaments", new { id = departamentoId });
            }

            int empleadosImportados = 0;
            List<string> errores = new List<string>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                int fila = 0;

                while (!reader.EndOfStream)
                {
                    var linea = await reader.ReadLineAsync();
                    fila++;

                    // Saltar cabecera
                    if (fila == 1)
                        continue;

                    var valores = linea.Split(',');

                    try
                    {
                        var model = new CreateEmpleadoViewModel
                        {
                            DepartamentoId = departamentoId,
                            Nombre = valores[0],
                            Apellidos = valores[1],
                            DNI = valores[2],
                            EmailCorporativo = valores[3],
                            FechaNacimiento = DateOnly.Parse(valores[4]),
                            NumSeguridadSocial = valores[5],
                            FechaAntiguedad = DateOnly.Parse(valores[6]),
                            GrupoCotizacion = (GrupoCotizacion)int.Parse(valores[7]),
                            SalarioBrutoAnual = decimal.Parse(valores[8]),
                            IBAN = valores[9],
                            EstadoCivil = GetEstadoCivil(valores[10]),
                            NumeroHijos = int.Parse(valores[11]),
                            PorcentajeDiscapacidad = int.Parse(valores[12])
                        };

                        var context = new ValidationContext(model, null, null);
                        var results = new List<ValidationResult>();

                        if (!Validator.TryValidateObject(model, context, results, true))
                        {
                            errores.Add($"Fila {fila}: Datos inválidos.");
                            continue;
                        }

                        var empleado = new Empleado
                        {
                            DepartamentoId = model.DepartamentoId,
                            Nombre = model.Nombre,
                            Apellidos = model.Apellidos,
                            Dni = model.DNI,
                            EmailCorporativo = model.EmailCorporativo,
                            FechaNacimiento = model.FechaNacimiento,
                            NumSeguridadSocial = model.NumSeguridadSocial,
                            FechaAntiguedad = model.FechaAntiguedad,
                            GrupoCotizacion = (int)model.GrupoCotizacion,
                            SalarioBrutoAnual = model.SalarioBrutoAnual,
                            Iban = model.IBAN?.Replace(" ", "").ToUpper(),
                            EstadoCivil = (int)model.EstadoCivil,
                            NumeroHijos = model.NumeroHijos,
                            PorcentajeDiscapacidad = model.PorcentajeDiscapacidad,
                            Activo = (model.Activo == EstadoEmpleado.Activo)
                        };

                        bool exito = await repoEmpleados.CreateEmpleadoAsync(empleado);

                        if (exito)
                        {
                            empleadosImportados++;
                        }
                        else
                        {
                            errores.Add($"Fila {fila}: Error al insertar en base de datos.");
                        }

                    }
                    catch (Exception ex)
                    {
                        errores.Add($"Fila {fila}: {ex.Message}");
                    }
                }
            }

            if (errores.Any())
            {
                TempData["ErrorMessage"] = $"Importación completada con errores. Insertados: {empleadosImportados}";
            }
            else
            {
                TempData["SuccessMessage"] = $"Se importaron {empleadosImportados} empleados correctamente.";
            }

            return RedirectToAction("Details", "Departaments", new { id = departamentoId });

        }

        [HttpGet]
        public IActionResult DescargarPlantillaCsv(int departamentoId)
        {
            var csv = new StringBuilder();

            csv.AppendLine("Nombre,Apellidos,DNI,EmailCorporativo,FechaNacimiento,NumSeguridadSocial,FechaAntiguedad,GrupoCotizacion,SalarioBrutoAnual,IBAN,EstadoCivil,NumeroHijos,PorcentajeDiscapacidad");

            csv.AppendLine("Juan,Perez,12345678A,juan@empresa.com,1990-05-10,123456789012,2022-01-01,5,25000,ES7620770024003102575766,1,0,0");

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "PlantillaEmpleados.csv");
        }

        private EstadoCivil GetEstadoCivil(string estadoCivilStr)
        {
            return estadoCivilStr.ToLower() switch
            {
                "soltero" => EstadoCivil.Soltero,
                "casado" => EstadoCivil.Casado,
                "divorciado" => EstadoCivil.Divorciado,
                "viudo" => EstadoCivil.Viudo,
                _ => throw new ArgumentException($"Estado Civil desconocido: {estadoCivilStr}")
            };
        }

    }
}
