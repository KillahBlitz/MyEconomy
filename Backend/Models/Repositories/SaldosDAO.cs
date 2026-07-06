using Backend.Data;
using Backend.Models;
using Backend.Models.Response;


namespace Backend.Models.Repositories
{
    public class SaldosDAO
    {
        private readonly AppDbContext _context;
        public SaldosDAO(AppDbContext context)
        {
            _context = context;
        }

        public DashboardContabilityResponse GenerateDashboard(int userId, DateOnly startDate, DateOnly endDate,
                                                bool saldo, List<int>? categories)
        {
            float totalSaldoGasto = 0f, totalSaldoIngreso = 0f, porcentajeGasto = 100f;

            var baseQuery = _context.Saldos.Where(s => s.id_usuario == userId && s.fecha >= startDate && s.fecha <= endDate);

            var saldoGastosQuery = baseQuery;
            if (categories is { Count: > 0 })
            {
                saldoGastosQuery = saldoGastosQuery.Where(s => s.id_categoria.HasValue && categories.Contains(s.id_categoria.Value) && s.id_categoria != null);
            }
            else
            {
                saldoGastosQuery = saldoGastosQuery.Where(s => s.id_categoria != null);
            }
            var saldoGastos = saldoGastosQuery.ToList();

            var saldoIngresos = saldo ? baseQuery.Where(s => s.id_categoria == null).ToList() : new List<SaldoModel>();

            foreach (var gasto in saldoGastos)
            {
                totalSaldoGasto += (float)gasto.cantidad;
            }
            foreach (var ingreso in saldoIngresos)
            {
                if(saldo) totalSaldoIngreso += (float)ingreso.cantidad;
            }

            porcentajeGasto = totalSaldoGasto > 0  ? totalSaldoIngreso / totalSaldoGasto * 100 : 100f;
            porcentajeGasto = porcentajeGasto > 100f ? 100f : porcentajeGasto;

            return new DashboardContabilityResponse
            {
                userId = userId,
                totalSaldoGasto = totalSaldoGasto,
                totalSaldoIngreso = totalSaldoIngreso,
                porcentajeGasto = porcentajeGasto,
                saldoGastos = saldoGastos,
                saldoIngresos = saldoIngresos,
                success = true,
                error = string.Empty
            };
        }

        public bool CreateSaldo(short userId, string concepto, decimal cantidad, int id_categoria)
        {
            try
            {
                var fecha = DateOnly.FromDateTime(DateTime.Now);
                var categoria = id_categoria != 0 ? (short?)id_categoria : null;
                var saldo = new SaldoModel
                {
                    id_usuario = userId,
                    concepto = concepto,
                    cantidad = cantidad,
                    fecha = fecha,
                    id_categoria = categoria
                };

                _context.Saldos.Add(saldo);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteSaldo(int saldoId)
        {
            try
            {
                var saldo = _context.Saldos.FirstOrDefault(s => s.saldo_id == saldoId);
                if (saldo != null)
                {
                    _context.Saldos.Remove(saldo);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}