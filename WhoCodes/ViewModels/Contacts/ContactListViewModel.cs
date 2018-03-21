using System;
using WhoCodes.Models;
using WhoCodes.ViewModels.Companies;

namespace WhoCodes.ViewModels.Contacts
{
    public class ContactListViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public int? CompanyId { get; set; }

        public CompanyListViewModel Company { get; private set; }

        public ContactListViewModel(Contact entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            Id = entity.Id;
            FirstName = entity.FirstName;
            LastName = entity.LastName;
            Phone = entity.Phone;
            Email = entity.Email;
            CompanyId = entity.CompanyId;

            Company = entity.Company != null ? new CompanyListViewModel(entity.Company) : null;
        }
    }
}
