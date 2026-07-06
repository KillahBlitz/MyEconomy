using Backend.Data;
using Backend.Models;
using Backend.Models.Resquest;
using Backend.Models.Response;
namespace Backend.Models.Repositories
{
    public class TicketsDAO
    {
        private readonly AppDbContext _context;
        public TicketsDAO(AppDbContext context)
        {
            _context = context;
        }

        public bool CreateTicket(short userId, List<ProductoRequest> products)
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return false;
            }

            var ticket = new TicketModel
            {
                id_usuario = userId,
                fcha_emision = DateOnly.FromDateTime(DateTime.Now),
                cuenta_total = 0
            };

            foreach (var product in products)
            {
                ticket.cuenta_total += product.precio_unitario * product.cantidad;
            }

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            foreach (var product in products)
            {
                var productDB = new ProductoModel
                {
                    id_ticket = ticket.ticket_id,
                    descripcion = product.descripcion,
                    cantindad = product.cantidad,
                    precio_unitaro = product.precio_unitario
                };
                _context.Productos.Add(productDB);
            }

            _context.SaveChanges();

            return true;
        }

        public bool DeleteTicket(int ticketId)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.ticket_id == ticketId);
            if (ticket == null)
            {
                return false;
            }

            var products = _context.Productos.Where(p => p.id_ticket == ticketId).ToList();
            _context.Productos.RemoveRange(products);
            _context.Tickets.Remove(ticket);
            _context.SaveChanges();

            return true;
        }

        public List<TicketModel> GetTickets(short userId)
        {
            var tickets = _context.Tickets.Where(t => t.id_usuario == userId).ToList();
            if (tickets.Count == 0)
            {
                return [];
            }
            return tickets;
        }

        public TicketDetailResponse GetTicketDetail(int ticketId)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.ticket_id == ticketId);
            if (ticket == null)
            {
                return null;
            }

            var products = _context.Productos.Where(p => p.id_ticket == ticketId).ToList();
            var productRequests = products.Select(p => new ProductoResponse
            {
                descripcion = p.descripcion,
                precio_unitario = p.precio_unitaro,
                cantidad = p.cantindad
            }).ToList();

            return new TicketDetailResponse
            {
                total = ticket.cuenta_total,
                fecha = ticket.fcha_emision,
                productoRequests = productRequests
            };
        }
    }
}