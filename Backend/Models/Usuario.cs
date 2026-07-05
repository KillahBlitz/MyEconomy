using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("user_id")]
        public short UserId { get; set; }

        [Required]
        [Column("nombre_user")]
        [StringLength(20)]
        public string? NombreUsuario { get; set; }

        [Required]
        [Column("password")]
        [StringLength(10)]
        public string? Password { get; set; }
    }
}
