// Controllers/MyController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MyWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {
         private readonly ILogger<MyController> _logger;
        private readonly MyDbContext _context;

        public MyController(MyDbContext context, ILogger<MyController> logger)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MyModel>>> Get()
        {
            return await _context.MyModels.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<MyModel>> Post(MyModel model)
        {
            _context.MyModels.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }
    }
}