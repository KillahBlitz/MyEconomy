using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Productos")]
    public class ProductoModel
    {
        [Key]
        [Column("producto_id")]
        public int producto_id { get; set; }

        [Required]
        [Column("id_ticket")]
        public int id_ticket { get; set; }

        [Column("descripcion")]
        [StringLength(30)]
        public string? descripcion { get; set; }

        [Column("cantindad")]
        public short cantindad { get; set; }

        [Column("precio_unitaro")]
        public short precio_unitaro { get; set; }
    }
}