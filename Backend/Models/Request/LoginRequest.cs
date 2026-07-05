using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Resquest
{
    public class LoginRequest
    {
        [Required]
        [StringLength(10)]
        public string? Usuario { get; set; }
        
        [Required]
        [StringLength(20)]      
        public string? Password { get; set; }
    }
}
