using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Xml.Linq;

namespace ZohoXmlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        [HttpPost("receive")]
        public IActionResult ReceiveFromZoho([FromBody] JsonElement invoice)
        {
            try
            {
                // قراءة البيانات من JSON
                var invoiceId = invoice.GetProperty("invoice_id").GetString();
                var invoiceNo = invoice.GetProperty("invoice_no").GetString();
                var customerName = invoice.GetProperty("customer_name").GetString();
                var date = invoice.GetProperty("date").GetString();
                var total = invoice.GetProperty("total").GetDecimal();
                var tax = invoice.TryGetProperty("tax", out var taxVal) ? taxVal.GetDecimal() : 0;
                var totalWithTax = total + tax;

                // تعريف الـ namespaces
                XNamespace ns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

                // بناء XML
                var xml = new XDocument(
                    new XElement(ns + "Invoice",
                        new XAttribute(XNamespace.Xmlns + "cbc", cbc),
                        new XAttribute(XNamespace.Xmlns + "cac", cac),

                        new XElement(cbc + "ProfileID", "reporting:1.0"),
                        new XElement(cbc + "ID", invoiceNo),
                        new XElement(cbc + "UUID", Guid.NewGuid().ToString()),
                        new XElement(cbc + "IssueDate", date),
                        new XElement(cbc + "InvoiceTypeCode", new XAttribute("name", "012"), "388"),
                        new XElement(cbc + "DocumentCurrencyCode", "JOD"),
                        new XElement(cbc + "TaxCurrencyCode", "JOD"),

                        new XElement(cac + "AccountingCustomerParty",
                            new XElement(cac + "Party",
                                new XElement(cac + "PartyLegalEntity",
                                    new XElement(cbc + "RegistrationName", customerName)
                                )
                            )
                        ),

                        new XElement(cac + "LegalMonetaryTotal",
                            new XElement(cbc + "PayableAmount",
                                new XAttribute("currencyID", "JOD"),
                                totalWithTax.ToString("F2")
                            )
                        )
                    )
                );

                // تحويل XML إلى نص
                var xmlString = xml.Declaration?.ToString() + "\n" + xml.ToString();
                Console.WriteLine("📄 XML GENERATED:");
                Console.WriteLine(xmlString);

                return Ok(new
                {
                    message = "✅ تم توليد XML بنجاح",
                    xml = xmlString
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ Error: {ex.Message}");
            }
        }

    }
}
