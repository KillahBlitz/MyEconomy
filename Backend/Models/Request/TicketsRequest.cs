using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Resquest
{

    public class ProductoRequest
    {
        [Required]
        public string descripcion { get; set; }

        [Required]
        public short precio_unitario { get; set; }

        [Required]
        public short cantidad { get; set; }
    }

    public class CreateTicketRequest
    {
        [Required]
        public short user_id { get; set; }

        [Required]
        public List<ProductoRequest> products { get; set; } = new List<ProductoRequest>();
    }
}
