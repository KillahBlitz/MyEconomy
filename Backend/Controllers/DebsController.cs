using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.Models.Repositories;
using Backend.Models.Response;
using Backend.Models.Resquest;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class DebsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private DeudasDAO deudasDAO;

        public DebsController(AppDbContext context)
        {
            _context = context;
            deudasDAO = new DeudasDAO(_context);
        }

        private static bool IsInvalidRequest(DebsRequest? deudas)
        {
            return deudas == null
                   || deudas.UserId <= 0
                   || deudas.startDate == default
                   || deudas.endDate == default;
        }

        [HttpPost("GetDashboardDebs")]
        public DashboardDebsResponse GetDashboard([FromBody] DebsRequest Deudas)
        {
            var dashboard = new DashboardDebsResponse
            {
                userId = Deudas.UserId,
                totalSaldoCubierto = 0f,
                totalSaldoDeuda = 0f,
                porcentajeGasto = 100f,
                Deudas = new List<DeudaModel>(),
                Cubierto = new List<DeudaModel>(),
                success = false,
                error = "Parametros de busqueda invalidos"
            };

            if (IsInvalidRequest(Deudas))
            {
                return dashboard;
            }

            dashboard = deudasDAO.ObtenerDashboardDeudas(Deudas.UserId, Deudas.startDate, Deudas.endDate);
            return dashboard;

        }

        [HttpPost("Debs")]
        public bool CreateDebs([FromBody] DeudaModel deuda)
        {
            var result = deudasDAO.CreateDeuda(deuda);
            return result;
        }

        [HttpDelete("Debs/{id_deuda}")]
        public bool DeleteDebs(int id_deuda)
        {
            var result = deudasDAO.DeleteDeudas(id_deuda);
            return result;
        } 
    }
}
