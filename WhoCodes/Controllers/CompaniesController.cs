using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhoCodes.Models;
using WhoCodes.ViewModels;
using WhoCodes.ViewModels.Companies;
using WhoCodes.ViewModels.Skills;

namespace WhoCodes.Controllers
{
    [Route("api/[controller]")]
    public class CompaniesController : Controller
    {
        private readonly WhoCodesDbContext dbContext;

        public CompaniesController(WhoCodesDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Query companies.
        /// </summary>
        /// <param name="offset">Number of results to skip. (optional)</param>
        /// <param name="length">Maximum number of results. (optional)</param>
        /// <param name="sort">Sort field. (optional)</param>
        /// <param name="search">Search string to filter on. (optional)</param>
        /// <param name="skillId">SkillId filter. (optional)</param>
        /// <returns>Matching companies.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(QueryResult<CompanyListViewModel>), 200)]
        public async Task<IActionResult> GetAsync(
            [FromQuery(Name ="$offset")] int? offset,
            [FromQuery(Name = "$limit")] int? length,
            [FromQuery(Name = "$sort")] string sort,
            [FromQuery] string search,
            [FromQuery] int? skillId)
        {
            IQueryable<Company> query = dbContext.Set<Company>();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            if (skillId.HasValue)
            {
                query = query.Where(c => c.Contacts.Any(ct => ct.Skills.Any(s => s.SkillId == skillId.Value)));
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
                        query = query.OrderByDescending(c => c.Id);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(c => c.Id);
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

            return Ok(new QueryResult<CompanyListViewModel>
            {
                Results = results.Select(r => new CompanyListViewModel(r)),
                Count = count
            });
        }

        /// <summary>
        /// Gets a company by its id.
        /// </summary>
        /// <param name="id">Company id.</param>
        /// <returns>The company.</returns>
        [HttpGet("{id}", Name = "GetCompanyById")]
        [ProducesResponseType(typeof(CompanyListViewModel), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var company = await dbContext.Set<Company>().FindAsync(id);

            if (company != null)
            {
                return Ok(new CompanyListViewModel(company));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets a company details.
        /// </summary>
        /// <param name="id">Company id.</param>
        /// <returns>The company.</returns>
        [HttpGet("{id}/details", Name = "GetCompanyDetailsById")]
        [ProducesResponseType(typeof(CompanyDetailViewModel), 200)]
        public async Task<IActionResult> GetDetails(int id)
        {
            var contact = await dbContext.Set<Company>()
                .Include(nameof(Company.Contacts))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact != null)
            {
                var skills = await dbContext.Set<Skill>().Where(s => s.Contacts.Any(cs => cs.Contact.CompanyId == id)).ToListAsync();
                return Ok(new CompanyDetailViewModel(contact, skills));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Registers a new company.
        /// </summary>
        /// <param name="vm">The company to register.</param>
        /// <returns>Created company.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CompanyListViewModel), 201)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        public async Task<IActionResult> Post([FromBody]CompanyEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var company = new Company();
                vm.ApplyTo(company);

                dbContext.Set<Company>().Add(company);
                await dbContext.SaveChangesAsync();

                return CreatedAtRoute("GetCompanyById", new { id = company.Id }, new CompanyListViewModel(company));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing company.
        /// </summary>
        /// <param name="id">Updated company id.</param>
        /// <param name="vm">Updated company data.</param>
        /// <returns>The updated company.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CompanyListViewModel), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        public async Task<IActionResult> Put(int id, [FromBody]CompanyEditViewModel vm)
        {
            var company =  await dbContext.Set<Company>().FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                vm.ApplyTo(company);

                await dbContext.SaveChangesAsync();

                return AcceptedAtRoute("GetCompanyById", new { id = company.Id }, new CompanyListViewModel(company));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Removes an existing company.
        /// </summary>
        /// <param name="id">Removed company id.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var companySet = dbContext.Set<Company>();
            var company = await companySet.FindAsync(id);

            if (company != null)
            {
                companySet.Remove(company);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets company's skills.
        /// </summary>
        /// <param name="companyId">Company id.</param>
        /// <returns>Company's skills</returns>
        [HttpGet("{companyId}/Skills", Name = "GetCompanySkills")]
        [ProducesResponseType(typeof(IEnumerable<SkillListViewModel>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSkills(int companyId)
        {
            var company = await dbContext.Set<Company>().FindAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }
            else
            {
                var skills = await dbContext.Set<Skill>().Where(s => s.Contacts.Any(cs => cs.Contact.CompanyId == company.Id)).ToListAsync();
                return Ok(skills.Select(s => new SkillListViewModel(s)));
            }
        }
    }
}
