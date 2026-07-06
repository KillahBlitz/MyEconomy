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
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private TicketsDAO ticketsDAO;

        public TicketsController(AppDbContext context)
        {
            _context = context;
            ticketsDAO = new TicketsDAO(_context);
        }

        [HttpPost("ticket")]
        public bool CreateTicket([FromBody] CreateTicketRequest ticket)
        {
            var ticketDB = ticketsDAO.CreateTicket(ticket.user_id, ticket.products);
            if (ticketDB == null)
            {
                return false;
            }
            return true;
        }

        [HttpDelete("ticket/{ticketId}")]
        public bool DeleteTicket(int ticketId)
        {
            var ticketDB = ticketsDAO.DeleteTicket(ticketId);
            if (!ticketDB)
            {
                return false;
            }
            return true;
        }
        
        [HttpGet("tickets/{userId}")]
        public List<TicketModel> GetTickets(short userId)
        {
            var tickets = ticketsDAO.GetTickets(userId);
            return tickets;
        }

        [HttpGet("ticketDetail/{ticketId}")]
        public TicketDetailResponse GetTicketDetail(int ticketId)
        {
            var ticketDetail = ticketsDAO.GetTicketDetail(ticketId);
            return ticketDetail;
        }
    } 
}
