using SmartInventoryManagementSystem.Application.DTO.SaleDTO;
using SmartInventoryManagementSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartInventoryManagementSystem.Infrastructure.PDF
{
    public class BuildInvoice
    {
        public static string Build(GetByID sale)
        {
            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; font-size: 12px; }}
        .header {{ text-align: center; margin-bottom: 20px; }}
        .meta {{ margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; }}
        th {{ background-color: #f4f4f4; }}
        .total {{ text-align: right; font-weight: bold; }}
    </style>
</head>
<body>

<div class='header'>
    <h2>Invoice</h2>
</div>

<div class='meta'>
    <p><strong>Invoice #:</strong> {sale.SaleId}</p>
    <p><strong>Sale Date:</strong> {sale.SaleDate:yyyy-MM-dd}</p>
    <p><strong>Buyer:</strong> {sale.BuyerName}</p>
</div>

<table>
    <thead>
        <tr>
            <th>Product</th>
            <th>Quantity</th>
            <th>Unit Price</th>
            <th>Total</th>
        </tr>
    </thead>
    <tbody>
");

            foreach (var item in sale.Items)
            {
                sb.Append($@"
<tr>
    <td>{item.ProductName}</td>
    <td>{item.QuantitySold}</td>
    <td>{item.ProductPrice:C}</td>
    <td>{item.LineTotal:C}</td>
</tr>");
            }

            sb.Append($@"
    </tbody>
</table>

<p class='total'>Grand Total: {sale.TotalAmount:C}</p>

</body>
</html>");

            return sb.ToString();
        }

        // New method - works directly with Sale entity
        public static string BuildFromEntity(Sale sale)
        {
            var sb = new StringBuilder();

            sb.Append($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; font-size: 12px; }}
        .header {{ text-align: center; margin-bottom: 20px; }}
        .meta {{ margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; }}
        th {{ background-color: #f4f4f4; }}
        .total {{ text-align: right; font-weight: bold; }}
    </style>
</head>
<body>

<div class='header'>
    <h2>Invoice</h2>
</div>

<div class='meta'>
    <p><strong>Invoice #:</strong> {sale.SaleId}</p>
    <p><strong>Sale Date:</strong> {sale.SaleDate:yyyy-MM-dd}</p>
    <p><strong>Buyer:</strong> {sale.BuyerName}</p>
</div>

<table>
    <thead>
        <tr>
            <th>Product</th>
            <th>Quantity</th>
            <th>Unit Price</th>
            <th>Total</th>
        </tr>
    </thead>
    <tbody>
");

            foreach (var item in sale.SaleItems)
            {
                sb.Append($@"
<tr>
    <td>{item.Product?.ProductName}</td>
    <td>{item.Quantity}</td>
    <td>{item.UnitPrice:C}</td>
    <td>{item.LineTotal:C}</td>
</tr>");
            }

            sb.Append($@"
    </tbody>
</table>

<p class='total'>Grand Total: {sale.TotalAmount:C}</p>

</body>
</html>");

            return sb.ToString();
        }
    }
}
