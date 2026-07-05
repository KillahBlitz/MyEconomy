using Backend.Data;
using Backend.Models;
namespace Backend.Models.Repositories
{
    public class UsuarioDAO
    {
        private readonly AppDbContext _context;
        public UsuarioDAO(AppDbContext context)
        {
            _context = context;
        }

        public Usuario? Login(string nombreUsuario, string password)
        {
            var usuarioDB = _context.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario && u.Password == password);
            return usuarioDB;
        }
    }
}