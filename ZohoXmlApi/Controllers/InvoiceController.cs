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

                // توليد XML تجريبي
                var xml = new XDocument(
                    new XElement("Invoice",
                        new XAttribute(XNamespace.Xmlns + "cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
                        new XAttribute(XNamespace.Xmlns + "cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
                        new XAttribute(XNamespace.Xmlns + "", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"),

                        new XElement(XName.Get("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"), invoiceNo),
                        new XElement(XName.Get("UUID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"), Guid.NewGuid().ToString()),
                        new XElement(XName.Get("IssueDate", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"), date),
                        new XElement(XName.Get("InvoiceTypeCode", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
                            new XAttribute("name", "012"), "388"
                        ),

                        new XElement(XName.Get("AccountingCustomerParty", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
                            new XElement(XName.Get("Party", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
                                new XElement(XName.Get("PartyLegalEntity", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
                                    new XElement(XName.Get("RegistrationName", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"), customerName)
                                )
                            )
                        ),

                        new XElement(XName.Get("LegalMonetaryTotal", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"),
                            new XElement(XName.Get("PayableAmount", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
                                new XAttribute("currencyID", "JOD"), totalWithTax.ToString("F2")
                            )
                        )
                    )
                );

                // طباعة XML كنص
                var xmlString = xml.Declaration?.ToString() + "\n" + xml.ToString();
                Console.WriteLine("📄 XML GENERATED:");
                Console.WriteLine(xmlString);

                return Ok(new { message = "✅ XML تم توليده وطباعته", xml = xmlString });
            }
            catch (Exception ex)
            {
                return BadRequest($"❌ Error: {ex.Message}");
            }
        }

    }
}
