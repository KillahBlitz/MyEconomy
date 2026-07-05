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
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private UsuarioDAO usuarioDAO;

        public AuthController(AppDbContext context)
        {
            _context = context;
            usuarioDAO = new UsuarioDAO(_context);
        }

        [HttpPost("login")]
        public  LoginResponse login([FromBody] LoginRequest usuario)
        {
            string Error = "Usuario o contraseña incorrectos";
            LoginResponse response = new LoginResponse { Success = false, Error = Error, UserId = 0 };
            var usuarioDB = usuarioDAO.Login(usuario.Usuario, usuario.Password);
            if (usuarioDB == null)
            {
                return response;
            }
            response = new LoginResponse { Success = true, Error = null, UserId = usuarioDB.UserId };
            return response;
        }
    } 
}
