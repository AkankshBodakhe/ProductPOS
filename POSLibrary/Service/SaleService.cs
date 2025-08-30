using POSLibrary.Entities;
using POSLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSLibrary.Services
{
    public class SaleService
    {
        private readonly SaleRepository _repo = new SaleRepository();
        private List<Sale> _cachedSales;  // store once


        public int CreateSale(
            int salesmanId,
            string salesmanName,
            string customerName,
            string customerContact,
            PaymentMode mode,
            IEnumerable<SaleItem> items)
        {
            if (items == null || !items.Any())
                throw new InvalidOperationException("Cart is empty.");

            var grandTotal = items.Sum(i => i.FinalPrice);

            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                SalesmanId = salesmanId,
                SalesmanName = salesmanName,
                CustomerName = customerName ?? string.Empty,
                CustomerContact = customerContact ?? string.Empty,
                PaymentMode = mode,
                GrandTotal = grandTotal
            };

            return _repo.CreateSaleWithItems(sale, items.ToList());
        }

        // Load all sales into memory
        public List<object> GetAllSales(bool forceRefresh = false)
        {
            if (_cachedSales == null || forceRefresh)
            {
                _cachedSales = _repo.GetAll();
            }

            return ProjectSales(_cachedSales);
        }

        public decimal GetTodaysSalesTotal()
        {
            return _repo.GetTotalSalesForDate(DateTime.Today);
        }

        // In-memory filtering (instead of DB calls)
        public List<object> SearchSales(string customerName = null, int? invoiceNo = null, DateTime? from = null, DateTime? to = null)
        {
            var sales = _cachedSales ?? _repo.GetAll();

            var filtered = sales.AsEnumerable();

            if (from.HasValue && to.HasValue)
            {
                filtered = filtered.Where(s => s.SaleDate.Date >= from.Value.Date &&
                                               s.SaleDate.Date <= to.Value.Date);
            }

            if (!string.IsNullOrEmpty(customerName))
            {
                filtered = filtered.Where(s =>
                    !string.IsNullOrEmpty(s.CustomerName) &&
                    s.CustomerName.IndexOf(customerName, StringComparison.OrdinalIgnoreCase) >= 0);
            }


            if (invoiceNo.HasValue)
            {
                filtered = filtered.Where(s => s.Id == invoiceNo.Value);
            }

            return ProjectSales(filtered.ToList());
        }

        // Projection method
        private List<object> ProjectSales(List<Sale> sales)
        {
            return sales.Select(s => new
            {
                InvoiceNo = s.Id,
                SaleDate = s.SaleDate,
                Customer = s.CustomerName,
                Salesman = s.SalesmanName,
                PaymentMode = s.PaymentMode.ToString(),
                GrandTotal = s.GrandTotal,
                Products = string.Join(", ", s.SaleItems.Select(i => $"{i.ProductName} (x{i.Quantity})"))
            }).Cast<object>().ToList();
        }

        public List<SaleItem> GetSaleItemsBySaleId(int saleId)
        {
            return _repo.GetSaleItemsBySaleId(saleId);
        }


        public Sale getSalebySaleid(int saleId)
        {
            var sale = _repo.GetSalebyId(saleId);
            return new Sale
                {
                    Id = sale.Id,
                    SaleDate = sale.SaleDate,
                    SalesmanName = sale.SalesmanName,
                    CustomerName = sale.CustomerName,
                    CustomerContact = sale.CustomerContact,
                    PaymentMode = sale.PaymentMode,
                    GrandTotal = sale.GrandTotal
                };
        }

    }
}
