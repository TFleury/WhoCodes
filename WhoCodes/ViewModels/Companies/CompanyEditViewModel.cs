using System;
using System.ComponentModel.DataAnnotations;
using WhoCodes.Models;

namespace WhoCodes.ViewModels.Companies
{
    public class CompanyEditViewModel
    {
        [StringLength(128), Required]
        public string Name { get; set; }

        [StringLength(128)]
        public string Website { get; set; }

        public void ApplyTo(Company entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Name = Name;
            entity.Website = Website;
        }
    }
}
