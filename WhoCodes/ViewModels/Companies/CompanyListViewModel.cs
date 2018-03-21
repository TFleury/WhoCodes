using System;
using WhoCodes.Models;

namespace WhoCodes.ViewModels.Companies
{
    public class CompanyListViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Website { get; set; }

        public CompanyListViewModel(Company entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Id = entity.Id;
            Name = entity.Name;
            Website = entity.Website;
        }
    }
}
