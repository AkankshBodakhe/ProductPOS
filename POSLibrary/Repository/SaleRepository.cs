using POSLibrary.Data;
using POSLibrary.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace POSLibrary.Repositories
{
    public class SaleRepository
    {
        public int CreateSaleWithItems(Sale sale, List<SaleItem> items)
        {
            using (var ctx = new POSDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                try
                {
                    // attach items to sale
                    sale.SaleItems = items;

                    // stock checks + updates
                    foreach (var it in items)
                    {
                        var product = ctx.Products.SingleOrDefault(p => p.Id == it.ProductId);
                        if (product == null)
                            throw new InvalidOperationException($"Product {it.ProductId} not found.");

                        if (product.AvailableStock < it.Quantity)
                            throw new InvalidOperationException(
                                $"Insufficient stock for {product.Name}. Available: {product.AvailableStock}, requested: {it.Quantity}");

                        product.AvailableStock -= it.Quantity;
                        ctx.Entry(product).State = EntityState.Modified;
                    }

                    ctx.Sales.Add(sale);
                    ctx.SaveChanges(); // generates Sale.Id and inserts items in one go
                    tx.Commit();

                    return sale.Id; // invoice number
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public decimal GetTotalSalesForDate(DateTime date)
        {
            using (var ctx = new POSDbContext())
            {
                // Calculate the start and end of the day to create a date range
                DateTime startOfDay = date.Date;
                DateTime endOfDay = date.Date.AddDays(1);

                // Filter the sales within this date range
                return ctx.Sales
                    .Where(s => s.SaleDate >= startOfDay && s.SaleDate < endOfDay)
                    .Sum(s => (decimal?)s.GrandTotal) ?? 0M;
            }
        }
        public List<Sale> GetAll()
        {
            using (var ctx = new POSDbContext())
            {
                return ctx.Sales
                    .Include(s => s.SaleItems)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();
            }
        }

        public List<SaleItem> GetSaleItemsBySaleId(int saleId)
        {
            using (var ctx = new POSDbContext())
            {
                return ctx.SaleItems
                          .Where(si => si.SaleId == saleId)
                          .ToList();
            }
        }


    }
}
