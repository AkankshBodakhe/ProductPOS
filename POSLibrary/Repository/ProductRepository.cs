using POSLibrary.Data;
using POSLibrary.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace POSLibrary.Repositories
{

    public class ProductRepository
    {
        // Create
        public void Add(Product product)
        {
            using (var context = new POSDbContext())
            {
                context.Products.Add(product);
                context.SaveChanges();
            }
        }

        // Read (Get by Id)
        public Product GetById(int id)
        {
            using (var context = new POSDbContext())
            {
                return context.Products.FirstOrDefault(p => p.Id == id);
            }
        }

        // Read (Get all)
        public List<Product> GetAll()
        {
            using (var context = new POSDbContext())
            {
                return context.Products.ToList();
            }
        }

        // Update
        public void Update(Product product)
        {
            using (var context = new POSDbContext())
            {
                var existing = context.Products.Find(product.Id);
                if (existing != null)
                {
                    existing.Name = product.Name;
                    existing.Rate = product.Rate;
                    existing.DiscountPercent = product.DiscountPercent;
                    existing.AvailableStock = product.AvailableStock;

                    context.SaveChanges();
                }
            }
        }

        // Delete
        public void Delete(int id)
        {
            using (var context = new POSDbContext())
            {
                var product = context.Products.Find(id);
                if (product != null)
                {
                    context.Products.Remove(product);
                    context.SaveChanges();
                }
            }
        }
    }

}
