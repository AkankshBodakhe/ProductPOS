using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Xml.Linq;
using POSLibrary.Entities;

namespace POSApp.Helpers
{
    public static class PrinterHelper
    {
        private static string _printerName;
        private static bool _landscape;
        private static string _fontName;
        private static float _fontSize;
        private static int _marginLeft;
        private static int _marginTop;
        private static int _tableWidthPercent = 80;
        private static int _rowPadding = 20;
        private static int _totalsSpacing = 60;

        public static void LoadConfig(string configPath)
        {
            if (!System.IO.File.Exists(configPath))
                throw new System.IO.FileNotFoundException("Printer config file not found.", configPath);

            var doc = XDocument.Load(configPath);
            var root = doc.Element("configuration")?.Element("printerSettings");

            _printerName = root?.Element("printerName")?.Value ?? throw new Exception("printerName missing");
            _landscape = (root?.Element("pageOrientation")?.Value ?? "Portrait").Equals("Landscape", StringComparison.OrdinalIgnoreCase);
            _fontName = root?.Element("fontName")?.Value ?? "Arial";
            _fontSize = float.TryParse(root?.Element("fontSize")?.Value, out float f) ? f : 10f;
            _marginLeft = int.TryParse(root?.Element("marginLeft")?.Value, out int ml) ? ml : 50;
            _marginTop = int.TryParse(root?.Element("marginTop")?.Value, out int mt) ? mt : 50;
            _tableWidthPercent = int.TryParse(root?.Element("tableWidthPercent")?.Value, out int tw) ? tw : 80;
            _rowPadding = int.TryParse(root?.Element("rowPadding")?.Value, out int rp) ? rp : 20;
            _totalsSpacing = int.TryParse(root?.Element("totalsSpacing")?.Value, out int ts) ? ts : 60;
        }

        public static async Task PrintInvoiceAsync(Sale sale, List<SaleItem> items)
        {
            await Task.Run(() =>
            {
                PrintDocument pd = new PrintDocument
                {
                    PrinterSettings = { PrinterName = _printerName }
                };

                // Set exact A5 paper size (width, height in hundredths of an inch)
                var a5Width = 583; // 5.83 in
                var a5Height = 827; // 8.27 in
                pd.DefaultPageSettings.PaperSize = new PaperSize("A5", a5Width, a5Height);
                pd.DefaultPageSettings.Landscape = _landscape;

                pd.PrintPage += (sender, e) =>
                {
                    Graphics g = e.Graphics;
                    int y = _marginTop;

                    Font h1 = new Font(_fontName, _fontSize + 6, FontStyle.Bold);
                    Font h2 = new Font(_fontName, _fontSize, FontStyle.Regular);
                    Font hBold = new Font(_fontName, _fontSize, FontStyle.Bold);

                    int pageWidth = e.PageBounds.Width - 2 * _marginLeft;
                    int tableWidth = (int)(pageWidth * _tableWidthPercent / 100f);
                    int xStart = _marginLeft + (pageWidth - tableWidth) / 2;

                    // Draw page border
                    g.DrawRectangle(Pens.Black, _marginLeft / 2, _marginTop / 2, e.PageBounds.Width - _marginLeft, e.PageBounds.Height - _marginTop);

                    // Header
                    g.DrawString("INVOICE", h1, Brushes.Black, xStart + tableWidth / 2 - 40, y); y += 30;
                    g.DrawString($"Invoice No: {sale.Id}", h2, Brushes.Black, xStart, y); y += 15;
                    g.DrawString($"Date: {sale.SaleDate:dd-MMM-yyyy HH:mm}", h2, Brushes.Black, xStart, y); y += 15;
                    g.DrawString($"Salesman: {sale.SalesmanName}", h2, Brushes.Black, xStart, y); y += 15;
                    g.DrawString($"Customer: {sale.CustomerName}  |  Contact: {sale.CustomerContact}", h2, Brushes.Black, xStart, y); y += 15;
                    g.DrawString($"Payment Mode: {sale.PaymentMode}", h2, Brushes.Black, xStart, y); y += 20;

                    // Table header
                    string[] headers = { "Sr", "Product", "Qty", "Rate", "Disc %", "Price" };
                    int[] colWidths = ComputeColumnWidths(g, headers, items, tableWidth, h2);

                    StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };

                    // Draw table header
                    int x = xStart;
                    int rowHeight = 0;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        SizeF sz = g.MeasureString(headers[i], hBold, colWidths[i]);
                        rowHeight = Math.Max(rowHeight, (int)sz.Height + _rowPadding);
                    }
                    x = xStart;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        Rectangle rect = new Rectangle(x, y, colWidths[i], rowHeight);
                        g.FillRectangle(Brushes.LightGray, rect);
                        g.DrawRectangle(Pens.Black, rect);
                        sf.Alignment = StringAlignment.Center;
                        g.DrawString(headers[i], hBold, Brushes.Black, rect, sf);
                        x += colWidths[i];
                    }
                    y += rowHeight;

                    // Table rows
                    int sr = 1;
                    decimal grossTotal = 0, discountTotal = 0;
                    foreach (var item in items)
                    {
                        x = xStart;
                        string[] row = { sr.ToString(), item.ProductName, item.Quantity.ToString(), item.Rate.ToString("0.00"), item.DiscountPercent.ToString("0.##"), item.FinalPrice.ToString("0.00") };
                        int maxRowHeight = 0;

                        // compute row height
                        for (int i = 0; i < row.Length; i++)
                        {
                            SizeF sz = g.MeasureString(row[i], h2, colWidths[i]);
                            maxRowHeight = Math.Max(maxRowHeight, (int)sz.Height + _rowPadding);
                        }

                        for (int i = 0; i < row.Length; i++)
                        {
                            Rectangle rect = new Rectangle(x, y, colWidths[i], maxRowHeight);
                            g.DrawRectangle(Pens.Black, rect);
                            sf.Alignment = (i == 0 || i >= 2) ? StringAlignment.Far : StringAlignment.Near;
                            g.DrawString(row[i], h2, Brushes.Black, rect, sf);
                            x += colWidths[i];
                        }
                        y += maxRowHeight;

                        grossTotal += item.Rate * item.Quantity;
                        discountTotal += (item.Rate * item.Quantity - item.FinalPrice);
                        sr++;
                    }

                    // Totals and signature
                    string signature = "Authorized Signature";
                    SizeF sigSize = g.MeasureString(signature, h2);
                    float signatureY = e.PageBounds.Height - _marginTop - sigSize.Height;
                    float totalsY = signatureY - _totalsSpacing;

                    int totalsX = xStart + (int)(tableWidth * 0.55);
                    g.DrawString("Gross Total: " + grossTotal.ToString("0.00"), h2, Brushes.Black, totalsX, totalsY); totalsY += 15;
                    g.DrawString("Total Discount: " + discountTotal.ToString("0.00"), h2, Brushes.Black, totalsX, totalsY); totalsY += 15;
                    g.DrawString("Grand Total: " + sale.GrandTotal.ToString("0.00"), hBold, Brushes.Black, totalsX, totalsY);

                    // Draw signature
                    g.DrawString(signature, h2, Brushes.Black, e.PageBounds.Width - _marginLeft - sigSize.Width, signatureY);
                };

                pd.Print();
            });
        }

        // Compute column widths proportional to content
        private static int[] ComputeColumnWidths(Graphics g, string[] headers, List<SaleItem> items, int tableWidth, Font font)
        {
            int cols = headers.Length;
            int[] widths = new int[cols];
            for (int i = 0; i < cols; i++)
            {
                widths[i] = (int)g.MeasureString(headers[i], font).Width + 20; // header width + padding
            }

            foreach (var item in items)
            {
                string[] row = { item.Id.ToString(), item.ProductName, item.Quantity.ToString(), item.Rate.ToString("0.00"), item.DiscountPercent.ToString("0.##"), item.FinalPrice.ToString("0.00") };
                for (int i = 0; i < cols; i++)
                {
                    int w = (int)g.MeasureString(row[i], font).Width + 20;
                    if (w > widths[i]) widths[i] = w;
                }
            }

            int total = 0;
            foreach (var w in widths) total += w;
            float scale = tableWidth / (float)total;
            for (int i = 0; i < widths.Length; i++) widths[i] = (int)(widths[i] * scale);

            return widths;
        }
    }
}
