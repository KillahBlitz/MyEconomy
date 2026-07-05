using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Deudas")]
    public class DeudaModel
    {
        [Key]
        [Column("deuda_id")]
        public int deuda_id { get; set; }

        [Required]
        [Column("id_usuario")]
        public short id_usuario { get; set; }

        [Column("tipo_deuda")]
        public bool tipo_deuda { get; set; }

        [Column("descripcion")]
        [StringLength(100)]
        public string? descripcion { get; set; }

        [Column("cantidad", TypeName = "decimal(10,2)")]
        public decimal cantidad { get; set; }

        [Column("fcha_registro")]
        public DateOnly fcha_registro { get; set; }
    }
}