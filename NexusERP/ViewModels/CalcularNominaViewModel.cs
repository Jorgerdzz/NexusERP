using System.ComponentModel.DataAnnotations;

namespace NexusERP.ViewModels
{
    public class CalcularNominaViewModel
    {
        public int EmpleadoId { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }

        public string EmpleadoIniciales { get; set; }
        public string EmpleadoNombre { get; set; }
        public string DepartamentoNombre { get; set; }

        public decimal SalarioBrutoAnual { get; set; }
        public decimal SalarioMensualSugerido => SalarioBrutoAnual / 12;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaInicio { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaFin { get; set; }

        public decimal PorcentajeIRPF { get; set; }

        public List<ConceptoNominaItemViewModel> Conceptos { get; set; } = new List<ConceptoNominaItemViewModel>();

        // TOTALES TRABAJADOR
        public decimal TotalDevengado { get; set; }
        public decimal SS_ContingenciasComunes { get; set; }
        public decimal SS_Desempleo { get; set; }
        public decimal SS_MEI { get; set; }
        public decimal SS_Formacion { get; set; }
        public decimal RetencionIRPF { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal LiquidoAPercibir { get; set; }

        // TOTALES EMPRESA 
        public decimal SS_Empresa_ContingenciasComunes { get; set; }
        public decimal SS_Empresa_AccidentesTrabajo { get; set; }
        public decimal SS_Empresa_Desempleo { get; set; }
        public decimal SS_Empresa_Formacion { get; set; }
        public decimal SS_Empresa_Fogasa { get; set; }
        public decimal SS_Empresa_MEI { get; set; } 
        public decimal SS_Empresa_Total { get; set; }
        public decimal CosteTotalEmpresa { get; set; }
    }
}
