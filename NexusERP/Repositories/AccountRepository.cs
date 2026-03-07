using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Enums;
using NexusERP.Helpers;
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

        public async Task<(bool exito, string mensaje, Usuario? usuarioCreado)> RegisterUserAsync(RegistroViewModel model)
        {
            bool emailExiste = await this.context.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.Email == model.Email);
            if (emailExiste) return (false, "Email ya registrado en el sistema", null);

            bool cifExiste = await this.context.Empresas.IgnoreQueryFilters().AnyAsync(e => e.Cif == model.CIF);
            if (cifExiste) return (false, "Ya existe una empresa registrada con ese CIF", null);

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

                await this.context.Empresas.AddAsync(empresa);
                await this.context.SaveChangesAsync();

                CuentasContable cuenta640 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "6400000", Nombre = "Sueldos y Salarios", Tipo = "Gasto" };
                CuentasContable cuenta642 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "6420000", Nombre = "Seguridad Social a cargo de la empresa", Tipo = "Gasto" };
                CuentasContable cuenta476 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "4760000", Nombre = "Organismos de la Seguridad Social, acreedores", Tipo = "Pasivo" };
                CuentasContable cuenta4751 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "4751000", Nombre = "H.P. acreedora por retenciones practicadas", Tipo = "Pasivo" };
                CuentasContable cuenta465 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "4650000", Nombre = "Remuneraciones pendientes de pago", Tipo = "Pasivo" };
                CuentasContable cuenta430 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "4300000", Nombre = "Clientes", Tipo = "Activo" };
                CuentasContable cuenta477 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "4770000", Nombre = "H.P. IVA Repercutido", Tipo = "Pasivo" };
                CuentasContable cuenta572 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "5720000", Nombre = "Bancos e instituciones de crédito", Tipo = "Activo" };
                CuentasContable cuenta700 = new CuentasContable { EmpresaId = empresa.Id, Codigo = "7000000", Nombre = "Venta de mercaderías y servicios", Tipo = "Ingreso" };


                await this.context.CuentasContables.AddRangeAsync(cuenta640, cuenta476, cuenta4751, cuenta642, cuenta465);
                await this.context.SaveChangesAsync(); 

                List<ConceptosSalariale> conceptosBase = new List<ConceptosSalariale>()
                {
                    // Devengos Salariales (Tipo 1, Tributan IRPF)
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "SAL_BASE", Nombre = "Salario Base", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "INCENTIVOS", Nombre = "Incentivos", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_DED", Nombre = "Plus especial dedicación", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_ANT", Nombre = "Plus antigüedad", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_ACT", Nombre = "Plus actividad", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_NOC", Nombre = "Plus nocturnidad", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_RES", Nombre = "Plus responsabilidad", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_CONV", Nombre = "Plus convenio", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_IDIOM", Nombre = "Plus idiomas", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "H_EXTRA", Nombre = "Horas extraordinarias", Tipo = 1, TributaIrpf = true, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "SAL_ESP", Nombre = "Salario en especie", Tipo = 1, TributaIrpf = true, Activo = true },

                    // Devengos No Salariales (Tipo 1, NO Tributan IRPF)
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "IND_SUP", Nombre = "Indemnizaciones o Suplidos", Tipo = 1, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PREST_SS", Nombre = "Prestaciones S.S.", Tipo = 1, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "IND_DESP", Nombre = "Indemnizaciones por despido", Tipo = 1, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "PLUS_TRANS", Nombre = "Plus transporte", Tipo = 1, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta640.Id, Codigo = "DIETAS", Nombre = "Dietas", Tipo = 1, TributaIrpf = false, Activo = true },

                    // Deducciones (Tipo 2, NO Tributan - Son retenciones)
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta476.Id, Codigo = "PREST_SS", Nombre = "Prestaciones S.S.", Tipo = 2, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta476.Id, Codigo = "DED_CC", Nombre = "Contingencias Comunes", Tipo = 2, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta476.Id, Codigo = "DED_MEI", Nombre = "Mecanismo Equidad Intergeneracional", Tipo = 2, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta476.Id, Codigo = "DED_DES", Nombre = "Desempleo", Tipo = 2, TributaIrpf = false, Activo = true },
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta476.Id, Codigo = "DED_FP", Nombre = "Formación Profesional", Tipo = 2, TributaIrpf = false, Activo = true },
                    
                    new ConceptosSalariale { EmpresaId = empresa.Id, CuentaContableId = cuenta4751.Id, Codigo = "DED_IRPF", Nombre = "Retención I.R.P.F.", Tipo = 2, TributaIrpf = false, Activo = true }
                };

                await this.context.ConceptosSalariales.AddRangeAsync(conceptosBase);
                await this.context.SaveChangesAsync();

                Usuario user = new Usuario
                {
                    EmpresaId = empresa.Id,
                    Nombre = model.NombreUsuario,
                    Email = model.Email,
                    Rol = (int)RolesUsuario.Admin,
                    Activo = true,
                    Password = model.Password
                };

                await this.context.Usuarios.AddAsync(user);
                await this.context.SaveChangesAsync();

                SeguridadUsuario userSecurity = new SeguridadUsuario
                {
                    IdUsuario = user.Id,
                    Salt = HelperTools.GenerateSalt(),
                };

                userSecurity.PasswordHash = HelperCryptography.EncryptPassword(model.Password, userSecurity.Salt);

                await this.context.SeguridadUsuarios.AddAsync(userSecurity);
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

        public async Task<(bool acceso, string mensaje, Usuario? user)> LogInUserAsync(LoginViewModel model)
        {
            var datosLogin = await (from u in this.context.Usuarios.IgnoreQueryFilters()
                                    join s in this.context.SeguridadUsuarios.IgnoreQueryFilters() on u.Id equals s.IdUsuario
                                    join e in this.context.Empresas.IgnoreQueryFilters() on u.EmpresaId equals e.Id
                                    where u.Email == model.Email
                                    select new { Usuario = u, Seguridad = s, Empresa = e })
                                    .FirstOrDefaultAsync();

            if(datosLogin == null)
            {
                return (false, "El correo electrónico no está registrado.", null);
            }
            else
            {
                string saltBD = datosLogin.Seguridad.Salt;
                byte[] passwordHashBD = datosLogin.Seguridad.PasswordHash;

                byte[] passwordGenerado = HelperCryptography.EncryptPassword(model.Password, saltBD);

                bool esValido = HelperTools.CompareArrays(passwordGenerado, passwordHashBD);

                if (esValido)
                {
                    return (true, "Usuario logueado correctamente", datosLogin.Usuario);
                }
                else
                {
                    return (false, "La contraseña es incorrecta.", null);
                }
            }
            
        }

    }
}
