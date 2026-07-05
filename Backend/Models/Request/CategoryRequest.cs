using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models.Resquest
{
    public class CategoryCreateRequest
    {
        [Required]
        [StringLength(10)]
        public string? Categoria { get; set; }
        
    }

    public class CategoryDeleteRequest
    {
        [Required]
        public int CategoriaId { get; set; }
    }
}
