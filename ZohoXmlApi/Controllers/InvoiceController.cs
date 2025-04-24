using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ZohoXmlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        [HttpPost("receive")]
        public IActionResult ReceiveFromZoho([FromBody] JsonElement invoice)
        {
            var json = JsonSerializer.Serialize(invoice, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine("📦 Received invoice:");
            Console.WriteLine(json);

            // لاحقاً: توليد XML + رفعه للفوترة الأردنية
            return Ok(new { message = "تم الاستلام بنجاح ✅" });
        }
    }
}
