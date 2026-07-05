using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("categoria")]
    public class CategoriaModel
    {
        [Key]
        [Column("categoria_id")]
        public short categoria_id { get; set; }

        [Column("tipo_categoria")]
        [StringLength(10)]
        public string? tipo_categoria { get; set; }
    }
}