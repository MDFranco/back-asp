namespace WebAppiN5now.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using WebAppiN5now.Data;
    using WebAppiN5now.Model;
    using Nest;
    using Confluent.Kafka;
    using Newtonsoft.Json;

    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly IElasticClient _elasticClient;

        private readonly IProducer<Null, string> _kafkaProducer;
        
        public PermissionsController(ApplicationDbContext context, IElasticClient elasticClient, IProducer<Null, string> kafkaProducer)
        {
            _context = context;
            _elasticClient = elasticClient;
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _context.Permissions.ToListAsync();

            var kafkaMessage = new Message<Null, string> { Value = JsonConvert.SerializeObject(new { Id = Guid.NewGuid(), NameOperation = "get" }) };
            await _kafkaProducer.ProduceAsync("permission-operations", kafkaMessage);
            return Ok(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> RequestPermission([FromBody] Permission permission)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            var indexResponse = await _elasticClient.IndexDocumentAsync(permission);

            var kafkaMessage = new Message<Null, string> { Value = JsonConvert.SerializeObject(new { Id = Guid.NewGuid(), NameOperation = "request" }) };
            await _kafkaProducer.ProduceAsync("permission-operations", kafkaMessage);

            return Ok(permission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ModifyPermission(int id, [FromBody] Permission permission)
        {
            if (id != permission.Id) return BadRequest();

            _context.Entry(permission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                var updateResponse = await _elasticClient.UpdateAsync<Permission>(permission.Id, u => u.Doc(permission));

                var kafkaMessage = new Message<Null, string> { Value = JsonConvert.SerializeObject(new { Id = Guid.NewGuid(), NameOperation = "modify" }) };
                await _kafkaProducer.ProduceAsync("permission-operations", kafkaMessage);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Permissions.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }
    }
}
