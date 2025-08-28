using POSLibrary.Entities;
using POSLibrary.Repositories;
using System.Collections.Generic;

namespace POSLibrary.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repository;

        public ProductService()
        {
            _repository = new ProductRepository();
        }

        // Add new product
        public void AddProduct(Product product)
        {
            _repository.Add(product);
        }

        // Get product by Id
        public Product GetProductById(int id)
        {
            return _repository.GetById(id);
        }

        // Get all products
        public List<Product> GetAllProducts()
        {
            return _repository.GetAll();
        }

        // Update product
        public void UpdateProduct(Product product)
        {
            _repository.Update(product);
        }

        // Delete product
        public void DeleteProduct(int id)
        {
            _repository.Delete(id);
        }
    }
}
