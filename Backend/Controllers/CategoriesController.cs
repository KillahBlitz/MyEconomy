using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.Models.Repositories;
using Backend.Models.Response;
using Backend.Models.Resquest;


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private CategoriaDAO categoriaDAO;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
            categoriaDAO = new CategoriaDAO(_context);
        }

        [HttpGet("Categories")]
        public List<CategoriaModel> GetCategories()
        {
            return categoriaDAO.ObtenerCategorias();
        }

        [HttpPost("Category")]
        public bool CreateCategory([FromBody] CategoryCreateRequest categoria)
        {
            var categoriaDB = categoriaDAO.CrearCategoria(categoria.Categoria);
            if (categoriaDB == null)
            {
                return false;
            }
            return true;
        }

        [HttpDelete("Category")]
        public bool DeleteCategory([FromBody] CategoryDeleteRequest categoria)
        {
            var categoriaDB = categoriaDAO.EliminarCategoria(categoria.CategoriaId);
            if (categoriaDB == null)
            {
                return false;
            }
            return true;
        }
    } 
}
