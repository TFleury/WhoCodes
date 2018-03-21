using System;
using WhoCodes.Models;

namespace WhoCodes.ViewModels.Skills
{
    public class SkillListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public SkillListViewModel(Skill entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Id = entity.Id;
            Name = entity.Name;
        }
    }
}
