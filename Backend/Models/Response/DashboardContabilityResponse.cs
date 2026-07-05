using Backend.Models;

namespace Backend.Models.Response
{
    public class DashboardContabilityResponse
    {
        public int userId { get; set; }
        public float totalSaldoGasto { get; set; }
        public float totalSaldoIngreso { get; set; } = 0f;
        public float porcentajeGasto { get; set; } = 100f;
        public List<SaldoModel> saldoGastos { get; set; } = new();
        public List<SaldoModel> saldoIngresos { get; set; } = new();
        public bool success { get; set; } = false;
        public string error { get; set; } = "Parametros de busqueda invalidos";
    }
}
