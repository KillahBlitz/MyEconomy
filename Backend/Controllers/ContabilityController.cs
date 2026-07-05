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
    public class ContabilityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private SaldosDAO saldosDAO;

        public ContabilityController(AppDbContext context)
        {
            _context = context;
            saldosDAO = new SaldosDAO(_context);
        }

        private static bool IsNullOrEmpty<T>(IEnumerable<T>? values)
        {
            return values == null || !values.Any();
        }

        private static bool IsInvalidRequest(ContabilityRequest? contabilidad)
        {
            return contabilidad == null
                   || contabilidad.UserId <= 0
                   || contabilidad.startDate == default
                   || contabilidad.endDate == default;
        }

        [HttpPost("GetDashboardContability")]
        public DashboardContabilityResponse GetDashboard([FromBody] ContabilityRequest Contabilidad)
        {
            var dashboard = new DashboardContabilityResponse
            {
                userId = Contabilidad.UserId,
                totalSaldoGasto = 0f,
                totalSaldoIngreso = 0f,
                porcentajeGasto = 100f,
                saldoGastos = new List<SaldoModel>(),
                saldoIngresos = new List<SaldoModel>(),
                success = false,
                error = "Parametros de busqueda invalidos"
            };
            if (IsInvalidRequest(Contabilidad))
            {
                return dashboard;
            }

            var categories = IsNullOrEmpty(Contabilidad.categories) ? null : Contabilidad.categories;

            var dashboardDB = saldosDAO.GenerateDashboard(Contabilidad.UserId, Contabilidad.startDate, Contabilidad.endDate, 
                                                            Contabilidad.neededSaldo, categories);

            return dashboardDB;
            
        }

        [HttpPost("Saldo")]
        public bool CreateSaldo([FromBody] SaldoModel saldo)
        {
            if (saldo == null || saldo.id_usuario <= 0 || string.IsNullOrEmpty(saldo.concepto))
            {
                return false;
            }

            var saldoDB = saldosDAO.CreateSaldo(saldo.id_usuario, saldo.concepto, saldo.cantidad, (short)saldo.id_categoria);

            return saldoDB;
        }

        [HttpDelete("Saldo/{id}")]
        public bool DeleteSaldo(int id)
        {

            var saldoDB = saldosDAO.DeleteSaldo(id);

            return saldoDB;
        }
    }    
}
