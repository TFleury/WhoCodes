using System.Collections.Generic;
using System.Linq;
using WhoCodes.Models;
using WhoCodes.ViewModels.Skills;

namespace WhoCodes.ViewModels.Companies
{
    public class CompanyDetailViewModel : CompanyListViewModel
    {
        public IEnumerable<SkillListViewModel> Skills { get; private set; }

        public CompanyDetailViewModel(Company entity, IEnumerable<Skill> skills) : base(entity)
        {
            if (skills == null)
            {
                throw new System.ArgumentNullException(nameof(skills));
            }

            Skills = skills.Select(s => new SkillListViewModel(s));
        }
    }
}
