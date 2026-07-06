using Backend.Models;

namespace Backend.Models.Response
{
    public class DashboardDebsResponse
    {
        public int userId { get; set; }
        public float totalSaldoCubierto { get; set; }
        public float totalSaldoDeuda { get; set; }
        public float porcentajeGasto { get; set; } = 100f;
        public List<DeudaModel> Deudas { get; set; } = new();
        public List<DeudaModel> Cubierto { get; set; } = new();
        public bool success { get; set; } = false;
        public string error { get; set; } = "Parametros de busqueda invalidos";
    }
}