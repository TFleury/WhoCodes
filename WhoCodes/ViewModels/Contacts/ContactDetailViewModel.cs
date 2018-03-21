using System.Collections.Generic;
using System.Linq;
using WhoCodes.Models;
using WhoCodes.ViewModels.Skills;

namespace WhoCodes.ViewModels.Contacts
{
    public class ContactDetailViewModel : ContactListViewModel
    {
        public IEnumerable<SkillListViewModel> Skills { get; private set; }

        public ContactDetailViewModel(Contact entity) : base(entity)
        {
            Skills = entity.Skills != null ? entity.Skills.Select(cs => new SkillListViewModel(cs.Skill)) : null;
        }

        public ContactDetailViewModel(Contact entity, IEnumerable<Skill> skills) : this(entity)
        {
            if (skills == null)
            {
                throw new System.ArgumentNullException(nameof(skills));
            }

            Skills = skills.Select(s => new SkillListViewModel(s));
        }
    }
}
