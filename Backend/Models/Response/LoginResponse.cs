using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Response
{
    public class LoginResponse
    {
        [Required]
        public bool Success { get; set; }
        
        [StringLength(100)]
        public string? Error { get; set; }
        
        public int UserId { get; set; }
    }
}
