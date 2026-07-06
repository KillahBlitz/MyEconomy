using Backend.Data;
using Backend.Models;
using Backend.Models.Response;



namespace Backend.Models.Repositories
{
    public class DeudasDAO
    {
        private readonly AppDbContext _context;
        public DeudasDAO(AppDbContext context)
        {
            _context = context;
        }

        public DashboardDebsResponse ObtenerDashboardDeudas(int userId, DateOnly startDate, DateOnly endDate)
        {
            var totalSaldoCubierto = _context.Deudas
                .Where(d => d.id_usuario == userId && d.tipo_deuda == false && d.fcha_registro >= startDate && d.fcha_registro <= endDate)
                .Sum(d => d.cantidad);

            var totalSaldoDeuda = _context.Deudas
                .Where(d => d.id_usuario == userId && d.tipo_deuda == true && d.fcha_registro >= startDate && d.fcha_registro <= endDate)
                .Sum(d => d.cantidad);

            var porcentajeGasto = totalSaldoDeuda > 0 ? (totalSaldoCubierto / totalSaldoDeuda) * 100 : 100;

            var Deudas = _context.Deudas
                .Where(d => d.id_usuario == userId && d.tipo_deuda == true && d.fcha_registro >= startDate && d.fcha_registro <= endDate)
                .ToList();

            var Cubiertos = _context.Deudas
                .Where(d => d.id_usuario == userId && d.tipo_deuda == false && d.fcha_registro >= startDate && d.fcha_registro <= endDate)
                .ToList();

            return new DashboardDebsResponse
            {
                userId = userId,
                totalSaldoCubierto = (float)totalSaldoCubierto,
                totalSaldoDeuda = (float)totalSaldoDeuda,
                porcentajeGasto = (float)porcentajeGasto,
                Deudas = Deudas,
                Cubierto = Cubiertos,
                success = true,
                error = string.Empty
            };
        }
    
        public bool CreateDeuda(DeudaModel deuda)
        {
            _context.Deudas.Add(deuda);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteDeudas(int id_deuda)
        {
            var deuda = _context.Deudas.FirstOrDefault(d => d.deuda_id == id_deuda);
            if (deuda != null)
            {
                _context.Deudas.Remove(deuda);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}