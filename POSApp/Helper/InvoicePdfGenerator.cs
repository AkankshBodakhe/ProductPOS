using iTextSharp.text;
using iTextSharp.text.pdf;
using POSLibrary.Entities;
using System.Collections.Generic;
using System.IO;

namespace POSApp.Helpers
{
    public static class InvoicePdfGenerator
    {
        public static void Generate(string filePath, Sale sale, IEnumerable<SaleItem> items)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (var doc = new Document(PageSize.A5, 36, 36, 36, 36))
                {
                    var writer = PdfWriter.GetInstance(doc, stream);
                    writer.PageEvent = new PageBorder(); // Add border
                    doc.Open();

                    var h1 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    var h2 = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    var hBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                    // Header info
                    doc.Add(new Paragraph("INVOICE", h1));
                    doc.Add(new Paragraph($"Invoice No: {sale.Id}", h2));
                    doc.Add(new Paragraph($"Date: {sale.SaleDate:dd-MMM-yyyy HH:mm}", h2));
                    doc.Add(new Paragraph($"Salesman: {sale.SalesmanName}", h2));
                    doc.Add(new Paragraph($"Customer: {sale.CustomerName}  |  Contact: {sale.CustomerContact}", h2));
                    doc.Add(new Paragraph($"Payment Mode: {sale.PaymentMode}", h2));
                    doc.Add(new Paragraph(" "));

                    // Product Table
                    var table = new PdfPTable(6) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 8, 40, 12, 12, 12, 16 });

                    AddCell(table, "Sr", hBold, true);
                    AddCell(table, "Product", hBold, true);
                    AddCell(table, "Qty", hBold, true);
                    AddCell(table, "Rate", hBold, true);
                    AddCell(table, "Disc %", hBold, true);
                    AddCell(table, "Price", hBold, true);

                    int sr = 1;
                    decimal grossTotal = 0;
                    decimal discountTotal = 0;

                    foreach (var it in items)
                    {
                        decimal lineGross = it.Rate * it.Quantity;
                        decimal lineDiscount = lineGross - it.FinalPrice;

                        grossTotal += lineGross;
                        discountTotal += lineDiscount;

                        AddCell(table, sr.ToString(), h2);
                        AddCell(table, it.ProductName, h2);
                        AddCell(table, it.Quantity.ToString(), h2);
                        AddCell(table, it.Rate.ToString("0.00"), h2);
                        AddCell(table, it.DiscountPercent.ToString("0.##"), h2);
                        AddCell(table, it.FinalPrice.ToString("0.00"), h2);
                        sr++;
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph(" "));

                    // Totals
                    var totalsTable = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_RIGHT };
                    totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    totalsTable.AddCell(new PdfPCell(new Phrase("Gross Total", h2)) { Border = Rectangle.NO_BORDER });
                    totalsTable.AddCell(new PdfPCell(new Phrase(grossTotal.ToString("0.00"), h2)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    totalsTable.AddCell(new PdfPCell(new Phrase("Total Discount", h2)) { Border = Rectangle.NO_BORDER });
                    totalsTable.AddCell(new PdfPCell(new Phrase(discountTotal.ToString("0.00"), h2)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    totalsTable.AddCell(new PdfPCell(new Phrase("Grand Total", hBold)) { Border = Rectangle.TOP_BORDER });
                    totalsTable.AddCell(new PdfPCell(new Phrase(sale.GrandTotal.ToString("0.00"), hBold)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    doc.Add(totalsTable);

                    // Space + Signature
                    doc.Add(new Paragraph("\n\n"));
                    Paragraph sign = new Paragraph("Authorized Signature", h2) { Alignment = Element.ALIGN_RIGHT };
                    doc.Add(sign);

                    doc.Close();
                }
            }
        }

        private static void AddCell(PdfPTable t, string text, Font font, bool header = false)
        {
            var cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = header ? Element.ALIGN_CENTER : Element.ALIGN_LEFT,
                BackgroundColor = header ? new BaseColor(235, 235, 235) : null,
                Border = Rectangle.BOX // Border for all cells
            };
            t.AddCell(cell);
        }

        // Border around whole page
        class PageBorder : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfContentByte cb = writer.DirectContent;
                Rectangle rect = document.PageSize;

                rect.Left += document.LeftMargin / 2;
                rect.Right -= document.RightMargin / 2;
                rect.Top -= document.TopMargin / 2;
                rect.Bottom += document.BottomMargin / 2;

                cb.SetLineWidth(1f);
                cb.Rectangle(rect.Left, rect.Bottom, rect.Width, rect.Height);
                cb.Stroke();
            }
        }
    }
}
