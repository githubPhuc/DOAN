using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOAN.Data;
using DOAN.Models;

namespace DOAN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class testModelsController : ControllerBase
    {
        private readonly DOANContext _context;

        public testModelsController(DOANContext context)
        {
            _context = context;
        }

        // GET: api/testModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<testModel>>> GettestModel()
        {
            return await _context.testModel.ToListAsync();
        }

        // GET: api/testModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<testModel>> GettestModel(int id)
        {
            var testModel = await _context.testModel.FindAsync(id);

            if (testModel == null)
            {
                return NotFound();
            }

            return testModel;
        }

        // PUT: api/testModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PuttestModel(int id, testModel testModel)
        {
            if (id != testModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(testModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!testModelExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/testModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<testModel>> PosttestModel(testModel testModel)
        {
            _context.testModel.Add(testModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GettestModel", new { id = testModel.Id }, testModel);
        }

        // DELETE: api/testModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletetestModel(int id)
        {
            var testModel = await _context.testModel.FindAsync(id);
            if (testModel == null)
            {
                return NotFound();
            }

            _context.testModel.Remove(testModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool testModelExists(int id)
        {
            return _context.testModel.Any(e => e.Id == id);
        }
    }
}
