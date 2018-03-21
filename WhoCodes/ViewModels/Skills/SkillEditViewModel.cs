using System;
using System.ComponentModel.DataAnnotations;
using WhoCodes.Models;

namespace WhoCodes.ViewModels.Skills
{
    public class SkillEditViewModel
    {
        [StringLength(64), Required]
        public string Name { get; set; }

        public void ApplyTo(Skill entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Name = Name;
        }
    }
}
