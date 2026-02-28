using NexusERP.Models;

namespace NexusERP.ViewModels
{
    public class DepartamentoDetailsViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal PresupuestoAnual { get; set; }

        public int NumeroEmpleados { get; set; }
        public decimal SalarioPromedio { get; set; }

        public List<Empleado> Empleados { get; set; } = new List<Empleado>();
    }
}
