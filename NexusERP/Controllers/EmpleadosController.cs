using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using NexusERP.Enums;
using Microsoft.AspNetCore.Authorization;
using NexusERP.Models;
using NexusERP.Repositories;
using NexusERP.Services;
using NexusERP.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

namespace NexusERP.Controllers
{
    [Authorize(Policy = "ADMIN")]
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

        public async Task<IActionResult> Details(int id)
        {
            Empleado empleado = await this.repoEmpleados.FindEmpleadoAsync(id);
            return View(empleado);
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
                    AlertService.Success(TempData, $"El empleado {model.Nombre} se ha añadido correctamente a la plantilla.");
                    return RedirectToAction("Details", "Departaments", new { id = model.DepartamentoId });
                }
                else
                {
                    AlertService.Error(TempData, "Ocurrió un error inesperado al guardar en la base de datos.");
                }

            }
            else
            {
                AlertService.Warning(TempData, "Revisa los datos del formulario.Algunos campos no tienen el formato correcto.");
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
                AlertService.Error(TempData, $"Importación completada con errores. Insertados: {empleadosImportados}");
            }
            else
            {
                AlertService.Toast(TempData, $"Se importaron {empleadosImportados} empleados correctamente.");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool eliminado = await this.repoEmpleados.DeleteEmpleadoAsync(id);
            if (eliminado)
            {
                AlertService.Success(TempData, "El empleado ha sido eliminado correctamente.");
                return Ok();
            }
            else
            {
                return BadRequest("No se pudo eliminar. Es posible que el empleado tenga nóminas o datos contables asociados.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEmpleadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Extraemos el nombre exacto de los campos que están fallando
                var camposConError = ModelState.Where(ms => ms.Value.Errors.Any())
                                               .Select(ms => ms.Key)
                                               .ToList();

                string mensajeErrores = string.Join(", ", camposConError);

                // Hacemos que el SweetAlert nos chive exactamente qué falla
                AlertService.Warning(TempData, $"Fallo de validación en: {mensajeErrores}");
                return RedirectToAction("Index");
            }

            try
            {
                Empleado emp = await this.repoEmpleados.FindEmpleadoAsync(model.Id);

                if (emp == null)
                {
                    AlertService.Error(TempData, "El empleado que intentas modificar no existe.");
                    return RedirectToAction("Index");
                }

                emp.Nombre = model.Nombre;
                emp.Apellidos = model.Apellidos;
                emp.Dni = model.DNI;
                emp.EmailCorporativo = model.EmailCorporativo;
                emp.Telefono = model.Telefono;
                emp.FechaNacimiento = model.FechaNacimiento;

                emp.DepartamentoId = model.DepartamentoId;
                emp.NumSeguridadSocial = model.NumSeguridadSocial;
                emp.FechaAntiguedad = model.FechaAntiguedad;
                emp.GrupoCotizacion = model.GrupoCotizacion;
                emp.SalarioBrutoAnual = Convert.ToDecimal(model.SalarioBrutoAnual);
                emp.Iban = model.IBAN;
                emp.Activo = model.Activo;

                emp.EstadoCivil = model.EstadoCivil;
                emp.NumeroHijos = model.NumeroHijos;
                emp.PorcentajeDiscapacidad = model.PorcentajeDiscapacidad;

                await this.repoEmpleados.UpdateEmpleadoAsync(emp);
                AlertService.Toast(TempData, "Datos del empleado actualizados correctamente", "success");

            }
            catch
            {
                AlertService.Error(TempData, "Hubo un error al guardar los cambios en la base de datos.");
            }
            return RedirectToAction("Index");
        }


    [HttpGet]
    public IActionResult DescargarPlantillaCsvGlobal()
    {
        var csv = new StringBuilder();
        // Añadimos "Departamento" como primera columna
        csv.AppendLine("Departamento,Nombre,Apellidos,DNI,EmailCorporativo,FechaNacimiento,NumSeguridadSocial,FechaAntiguedad,GrupoCotizacion,SalarioBrutoAnual,IBAN,EstadoCivil,NumeroHijos,PorcentajeDiscapacidad");

        // Fila de ejemplo
        csv.AppendLine("Recursos Humanos,Juan,Perez,12345678A,juan@empresa.com,1990-05-10,123456789012,2022-01-01,5,25000,ES7620770024003102575766,1,0,0");

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "PlantillaGlobalEmpleados.csv");
    }

    // 2. EL BUSCADOR "INTELIGENTE" DE DEPARTAMENTOS
    private int? BuscarDepartamentoInteligente(string nombreCsv, List<Departamento> departamentosBD)
    {
        if (string.IsNullOrWhiteSpace(nombreCsv)) return null;

        // Función rápida para limpiar textos (quita tildes y lo pasa a minúsculas)
        string LimpiarTexto(string texto)
        {
            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower().Trim();
        }

        string textoLimpio = LimpiarTexto(nombreCsv);

        // Diccionario de Alias (Puedes añadir los que quieras)
        var alias = new Dictionary<string, string>
        {
            { "rrhh", "recursos humanos" },
            { "it", "informatica" },
            { "sistemas", "informatica" },
            { "admin", "administracion" },
            { "mkt", "marketing" }
        };

        // Si el usuario escribió un alias, lo traducimos al nombre real
        if (alias.ContainsKey(textoLimpio))
        {
            textoLimpio = alias[textoLimpio];
        }

        // Buscamos en la Base de Datos comparando los textos limpios
        var dept = departamentosBD.FirstOrDefault(d => LimpiarTexto(d.Nombre) == textoLimpio);

        return dept?.Id;
    }

    // 3. LA IMPORTACIÓN GLOBAL
    [HttpPost]
    public async Task<IActionResult> ImportCsvGlobal(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            AlertService.Error(TempData, "Debes seleccionar un archivo CSV.");
            return RedirectToAction("Index");
        }

        int empleadosImportados = 0;
        List<string> errores = new List<string>();

        var departamentosBD = await this.repoDepartamentos.GetDepartamentosAsync(); // Ajusta este método a tu repositorio

        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            int fila = 0;
            while (!reader.EndOfStream)
            {
                var linea = await reader.ReadLineAsync();
                fila++;

                if (fila == 1) continue; // Saltar cabecera

                var valores = linea.Split(',');

                try
                {
                    // Buscamos el departamento con la función inteligente (Índice 0)
                    int? idDept = BuscarDepartamentoInteligente(valores[0], departamentosBD);

                    if (idDept == null)
                    {
                        errores.Add($"Fila {fila}: El departamento '{valores[0]}' no existe en la empresa.");
                        continue;
                    }

                    // OJO: Los índices se desplazan +1 respecto a tu código anterior
                    var empleado = new Empleado
                    {
                        DepartamentoId = idDept.Value,
                        Nombre = valores[1],
                        Apellidos = valores[2],
                        Dni = valores[3],
                        EmailCorporativo = valores[4],
                        FechaNacimiento = DateOnly.Parse(valores[5]),
                        NumSeguridadSocial = valores[6],
                        FechaAntiguedad = DateOnly.Parse(valores[7]),
                        GrupoCotizacion = int.Parse(valores[8]),
                        SalarioBrutoAnual = decimal.Parse(valores[9]),
                        Iban = valores[10]?.Replace(" ", "").ToUpper(),
                        EstadoCivil = (int)GetEstadoCivil(valores[11]),
                        NumeroHijos = int.Parse(valores[12]),
                        PorcentajeDiscapacidad = int.Parse(valores[13]),
                        Activo = true
                    };

                    bool exito = await repoEmpleados.CreateEmpleadoAsync(empleado);

                    if (exito) empleadosImportados++;
                    else errores.Add($"Fila {fila}: Error al insertar en base de datos.");
                }
                catch (Exception ex)
                {
                    errores.Add($"Fila {fila}: Error de formato. Revisa fechas o números. ({ex.Message})");
                }
            }
        }

        if (errores.Any())
        {
            // Puedes mostrar los errores en el TempData o usar tu AlertService
            AlertService.Warning(TempData, errores.First());
        }
        else
        {
            AlertService.Toast(TempData, $"Se importaron {empleadosImportados} empleados correctamente.");
        }

        return RedirectToAction("Index");
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
