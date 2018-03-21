using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhoCodes.Models;
using WhoCodes.ViewModels;
using WhoCodes.ViewModels.Contacts;
using WhoCodes.ViewModels.Skills;

namespace WhoCodes.Controllers
{
    [Route("api/[controller]")]
    public class ContactsController : Controller
    {
        private readonly WhoCodesDbContext dbContext;

        public ContactsController(WhoCodesDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Query contacts.
        /// </summary>
        /// <param name="offset">Number of results to skip. (optional)</param>
        /// <param name="length">Maximum number of results. (optional)</param>
        /// <param name="sort">Sort field. (optional)</param>
        /// <param name="search">Search string to filter on. (optional)</param>
        /// <param name="companyId">Company id filter. (optional)</param>
        /// <param name="skillId">Skill id filter. (optional)</param>
        /// <returns>Matching contacts.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(QueryResult<ContactListViewModel>), 200)]
        public async Task<IActionResult> GetAsync(
            [FromQuery(Name ="$offset")] int? offset,
            [FromQuery(Name = "$limit")] int? length,
            [FromQuery(Name = "$sort")] string sort,
            [FromQuery] string search,
            [FromQuery] int? companyId,
            [FromQuery] int? skillId)
        {
            IQueryable<Contact> query = dbContext.Set<Contact>().Include(c => c.Company);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search) || c.Company.Name.Contains(search));
            }

            if (companyId.HasValue)
            {
                query = query.Where(c => c.CompanyId == companyId.Value);
            }

            if (skillId.HasValue)
            {
                query = query.Where(c => c.Skills.Any(s => s.SkillId == skillId.Value));
            }

            var count = await query.CountAsync();

            if (!string.IsNullOrEmpty(sort))
            {
                bool desc = sort.StartsWith('-');
                sort = sort.TrimStart('-', '+');
                switch (sort)
                {
                    case "firstName":
                        query = desc ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName);
                        break;
                    case "lastName":
                        query = desc ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName);
                        break;
                    case "company":
                        query = desc ? query.OrderByDescending(c => c.Company.Name) : query.OrderBy(c => c.Company.Name);
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

            return Ok(new QueryResult<ContactListViewModel>
            {
                Results = results.Select(r => new ContactListViewModel(r)),
                Count = count
            });
        }

        /// <summary>
        /// Gets a contact by its id.
        /// </summary>
        /// <param name="id">Contact id.</param>
        /// <returns>The contact.</returns>
        [HttpGet("{id}", Name = "GetContactById")]
        [ProducesResponseType(typeof(ContactListViewModel), 200)]
        public async Task<IActionResult> Get(int id)
        {
            var contact = await dbContext.Set<Contact>().Include(c => c.Company).FirstOrDefaultAsync(c => c.Id == id);

            if (contact != null)
            {
                return Ok(new ContactListViewModel(contact));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets a contact details.
        /// </summary>
        /// <param name="id">Contact id.</param>
        /// <returns>The contact.</returns>
        [HttpGet("{id}/details", Name = "GetContactDetailsById")]
        [ProducesResponseType(typeof(ContactDetailViewModel), 200)]
        public async Task<IActionResult> GetDetails(int id)
        {
            var contact = await dbContext.Set<Contact>()
                .Include(nameof(Contact.Company))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact != null)
            {
                var skills = await dbContext.Set<Skill>().Where(s => s.Contacts.Any(cs => cs.ContactId == contact.Id)).ToListAsync();
                return Ok(new ContactDetailViewModel(contact, skills));
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Registers a new contact.
        /// </summary>
        /// <param name="vm">The contact to register.</param>
        /// <returns>Created company.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ContactListViewModel), 201)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        public async Task<IActionResult> Post([FromBody]ContactEditViewModel vm)
        {
            if (ModelState.IsValid)
            {
                if (vm.CompanyId.HasValue)
                {
                    var company = await dbContext.Set<Company>().FindAsync(vm.CompanyId.Value);
                    if(company == null)
                    {
                        ModelState.AddModelError(nameof(ContactListViewModel.CompanyId), "Company not found.");
                        return BadRequest(ModelState);
                    }
                }

                var contact = new Contact();
                vm.ApplyTo(contact);

                dbContext.Set<Contact>().Add(contact);
                await dbContext.SaveChangesAsync();

                return CreatedAtRoute("GetContactById", new { id = contact.Id }, new ContactListViewModel(contact));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing contact.
        /// </summary>
        /// <param name="id">Updated contact id.</param>
        /// <param name="vm">Updated contact data.</param>
        /// <returns>The updated contact.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ContactListViewModel), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(SerializableError), 400)]
        public async Task<IActionResult> Put(int id, [FromBody]ContactEditViewModel vm)
        {
            var contact =  await dbContext.Set<Contact>().FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                if (vm.CompanyId.HasValue && vm.CompanyId != contact.CompanyId)
                {
                    var company = await dbContext.Set<Company>().FindAsync(vm.CompanyId.Value);
                    if (company == null)
                    {
                        ModelState.AddModelError(nameof(ContactListViewModel.CompanyId), "Company not found.");
                        return BadRequest(ModelState);
                    }
                }

                vm.ApplyTo(contact);

                await dbContext.SaveChangesAsync();

                return AcceptedAtRoute("GetContactById", new { id = contact.Id }, new ContactListViewModel(contact));
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Removes an existing contact.
        /// </summary>
        /// <param name="id">Removed contact id.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var contactSet = dbContext.Set<Contact>();
            var contact = await contactSet.FindAsync(id);

            if (contact != null)
            {
                contactSet.Remove(contact);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Gets contact's skills.
        /// </summary>
        /// <param name="contactId">Contact id.</param>
        /// <returns>Contact skills</returns>
        [HttpGet("{contactId}/Skills", Name = "GetContactSkills")]
        [ProducesResponseType(typeof(IEnumerable<SkillListViewModel>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSkills(int contactId)
        {
            var contact = await dbContext.Set<Contact>()
                .FirstOrDefaultAsync(e => e.Id == contactId);

            if (contact == null)
            {
                return NotFound();
            }
            else
            {
                var skills = await dbContext.Set<Skill>().Where(s => s.Contacts.Any(cs => cs.ContactId == contact.Id)).ToListAsync();
                return Ok(skills.Select(s => new SkillListViewModel(s)));
            }
        }

        /// <summary>
        /// Adds skill to contact
        /// </summary>
        /// <param name="contactId">Contact id.</param>
        /// <param name="skillId">Skill id.</param>
        /// <returns></returns>
        [HttpPost("{contactId}/Skills/{skillId}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PostSkill(int contactId, int skillId)
        {
            var contact = await dbContext.Set<Contact>()
                .Include(e => e.Skills).ThenInclude(e => e.Skill)
                .FirstOrDefaultAsync(e => e.Id == contactId);

            var skill = await dbContext.Set<Skill>().FindAsync(skillId);

            if (contact == null || skill == null)
            {
                return NotFound();
            }

            if (!contact.Skills.Any(cs => cs.SkillId == skill.Id))
            {
                contact.Skills.Add(new ContactSkill { SkillId = skill.Id });
            }

            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetContactSkills", new { id = contact.Id });
        }

        /// <summary>
        /// Remove skill from contact
        /// </summary>
        /// <param name="contactId">Contact id.</param>
        /// <param name="skillId">Skill id.</param>
        /// <returns></returns>
        [HttpDelete("{contactId}/Skills/{skillId}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSkill(int contactId, int skillId)
        {
            var contact = await dbContext.Set<Contact>()
                .Include(e => e.Skills).ThenInclude(e => e.Skill)
                .FirstOrDefaultAsync(e => e.Id == contactId);

            var skill = contact.Skills.FirstOrDefault(cs => cs.SkillId == skillId);

            if (contact == null || skill == null)
            {
                return NotFound();
            }

            contact.Skills.Remove(skill);

            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetContactSkills", new { id = contact.Id });
        }
    }
}
