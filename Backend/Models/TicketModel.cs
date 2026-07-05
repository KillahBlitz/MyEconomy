using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Tickets")]
    public class TicketModel
    {
        [Key]
        [Column("ticket_id")]
        public int ticket_id { get; set; }

        [Required]
        [Column("id_usuario")]
        public short id_usuario { get; set; }

        [Column("cuenta_total", TypeName = "decimal(8,2)")]
        public decimal cuenta_total { get; set; }

        [Column("fcha_emision")]
        public DateOnly fcha_emision { get; set; }
    }
}