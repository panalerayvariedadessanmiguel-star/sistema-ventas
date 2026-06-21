using System.Collections.Generic;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuarioService()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        public List<Usuario> GetAll() => _usuarioRepository.GetAll();

        public Usuario GetById(int id) => _usuarioRepository.GetById(id);

        public Usuario GetByDocumento(string documento) => _usuarioRepository.GetByDocumento(documento);

        public int Insert(Usuario usuario) => _usuarioRepository.Insert(usuario);

        public bool Update(Usuario usuario) => _usuarioRepository.Update(usuario);

        public bool Delete(int id) => _usuarioRepository.Delete(id);
    }
}
