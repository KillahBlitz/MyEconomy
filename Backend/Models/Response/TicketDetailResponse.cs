using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Response
{
        public class ProductoResponse
    {
        [Required]
        public string descripcion { get; set; }

        [Required]
        public short precio_unitario { get; set; }

        [Required]
        public short cantidad { get; set; }
    }

    public class TicketDetailResponse
    {
        public decimal total { get; set; }

        public DateOnly fecha { get; set; }

        public List<ProductoResponse> productoRequests { get; set; }
    }
}
