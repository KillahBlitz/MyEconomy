using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Saldos")]
    public class SaldoModel
    {
        [Key]
        [Column("saldo_id")]
        public int saldo_id { get; set; }

        [Column("concepto")]
        [StringLength(50)]
        public string? concepto { get; set; }

        [Column("cantidad", TypeName = "decimal(10,2)")]
        public decimal cantidad { get; set; }
        
        [Column("fcha_registro")]
        public DateOnly fecha { get; set; }

        [Column("id_usuario")]
        public short id_usuario { get; set; }

        [Column("id_categoria")]
        public short? id_categoria { get; set; }
    }
}