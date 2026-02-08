using DinkToPdf;
using DinkToPdf.Contracts;
using SmartInventoryManagementSystem.Application.DTO;
using SmartInventoryManagementSystem.Application.Interfaces;
using SmartInventoryManagementSystem.Infrastructure.PDF;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SmartInventoryManagementSystem.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        private readonly IConverter _converter;

        public PdfService(IConverter converter)
        {
             _converter = converter;
        }

        public byte[] GenerateInvoicePdf(GetByID sale)
        {
            var html = BuildInvoice.Build(sale);

            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
        {
            PaperSize = PaperKind.A4,
            Orientation = Orientation.Portrait
        },
                Objects =
        {
            new ObjectSettings
            {
                HtmlContent = html
            }
        }
            };

            return _converter.Convert(doc);
        }
    }
}
