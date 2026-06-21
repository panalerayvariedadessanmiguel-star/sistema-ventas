using System.Collections.Generic;
using SistemaVentas.Data.Models;
using SistemaVentas.Data.Repositories;

namespace SistemaVentas.Business.Services
{
    public class CategoriaService
    {
        private readonly CategoriaRepository _repository;

        public CategoriaService()
        {
            _repository = new CategoriaRepository();
        }

        public List<Categoria> GetAll() => _repository.GetAll();

        public int Create(Categoria categoria) => _repository.Insert(categoria);
    }
}
