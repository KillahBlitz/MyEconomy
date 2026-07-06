using Backend.Data;
using Backend.Models;


namespace Backend.Models.Repositories
{
    public class CategoriaDAO
    {
        private readonly AppDbContext _context;
        public CategoriaDAO(AppDbContext context)
        {
            _context = context;
        }

        public CategoriaModel? CrearCategoria(string nombreCategoria)
        {
            var categoriaDB = new CategoriaModel{ tipo_categoria = nombreCategoria };
            var categoriaDBExist = _context.Categorias.FirstOrDefault(c => c.tipo_categoria == nombreCategoria);
            if (categoriaDBExist != null)
            {
                return null;
            }
            _context.Categorias.Add(categoriaDB);
            _context.SaveChanges();
            return categoriaDB;
        }

        public CategoriaModel? EliminarCategoria(int categoriaId)
        {
            var categoriaDB = _context.Categorias.FirstOrDefault(c => c.categoria_id == categoriaId);
            if (categoriaDB != null)
            {
                _context.Categorias.Remove(categoriaDB);
                _context.SaveChanges();
            }
            return categoriaDB;
        }

        public List<CategoriaModel> ObtenerCategorias()
        {
            return _context.Categorias.ToList();
        }
    }
}