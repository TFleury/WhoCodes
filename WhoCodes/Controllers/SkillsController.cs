using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WhoCodes.Models;
using WhoCodes.ViewModels;
using WhoCodes.ViewModels.Skills;

namespace WhoCodes.Controllers
{
    [Route("api/[controller]")]
    public class SkillsController : Controller
    {
        private readonly WhoCodesDbContext dbContext;

        public SkillsController(WhoCodesDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Query skills.
        /// </summary>
        /// <param name="offset">Number of results to skip. (optional)</param>
        /// <param name="length">Maximum number of results. (optional)</param>
        /// <param name="sort">Sort field. (optional)</param>
        /// <param name="search">Search string to filter on. (optional)</param>
        /// <returns>Matching companies.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(QueryResult<SkillListViewModel>), 200)]
        public async Task<IActionResult> GetAsync(
            [FromQuery(Name ="$offset")] int? offset,
            [FromQuery(Name = "$limit")] int? length,
            [FromQuery(Name = "$sort")] string sort,
            [FromQuery] string search)
        {
            IQueryable<Skill> query = dbContext.Set<Skill>();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            var count = await query.CountAsync();

            if (!string.IsNullOrEmpty(sort))
            {
                bool desc = sort.StartsWith('-');
                sort = sort.TrimStart('-', '+');
                switch (sort)
                {
                    case "name":
                        query = desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name);
                        break;
                    default:
                        query = query.OrderByDescending(s => s.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(s => s.Id);
            }

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (length.HasValue)
            {
                query = query.Take(length.Value);
            }

            var results = await query.ToListAsync();

            return Ok(new QueryResult<SkillListViewModel>
            {
                Results = results.Select(r => new SkillListViewModel(r)),
                Count = count
            });
        }

        /// <summary>
        /// Gets a skill by its id.
        /// </summary>
        /// <param name="id">Skill id.</param>
        /// <returns>The skill.</returns>
        [HttpGet("{id}", Name = "GetSkillById")]
        [ProducesResponseType(typeof(SkillListViewModel), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var skill = await dbContext.Set<Skill>().FindAsync(id);

            if (skill != null)
            {
                return Ok(new SkillListViewModel(skill));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Registers a new skill.
        /// </summary>
        /// <param name="vm">The skill to register.</param>
        /// <returns>Created skill.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(SkillListViewModel), 201)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        public async Task<IActionResult> Post([FromBody]SkillEditViewModel vm)
        {
            var skillSet = dbContext.Set<Skill>();
            var existing = await skillSet.FirstOrDefaultAsync(s => s.Name == vm.Name);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(SkillEditViewModel.Name), "A skill with the same name already exists.");
            }

            if (ModelState.IsValid)
            {
                var skill = new Skill();
                vm.ApplyTo(skill);

                skillSet.Add(skill);
                await dbContext.SaveChangesAsync();

                return CreatedAtRoute("GetSkillById", new { id = skill.Id }, new SkillListViewModel(skill));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing skill.
        /// </summary>
        /// <param name="id">Updated skill id.</param>
        /// <param name="vm">Updated skill data.</param>
        /// <returns>The updated skill.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SkillListViewModel), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        public async Task<IActionResult> Put(int id, [FromBody]SkillEditViewModel vm)
        {
            var skill =  await dbContext.Set<Skill>().FindAsync(id);

            if (skill == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                vm.ApplyTo(skill);

                await dbContext.SaveChangesAsync();

                return AcceptedAtRoute("GetSkillById", new { id = skill.Id }, new SkillListViewModel(skill));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Removes an existing skill.
        /// </summary>
        /// <param name="id">Removed skill id.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var skillSet = dbContext.Set<Skill>();
            var skill = await skillSet.FindAsync(id);

            if (skill != null)
            {
                skillSet.Remove(skill);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
