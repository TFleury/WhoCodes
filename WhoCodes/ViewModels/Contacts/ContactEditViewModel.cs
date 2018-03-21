using System;
using System.ComponentModel.DataAnnotations;
using WhoCodes.Models;

namespace WhoCodes.ViewModels.Contacts
{
    public class ContactEditViewModel
    {
        [StringLength(128), Required]
        public string FirstName { get; set; }

        [StringLength(128), Required]
        public string LastName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(256), Required]
        public string Email { get; set; }

        public int? CompanyId { get; set; }

        public void ApplyTo(Contact entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.FirstName = FirstName;
            entity.LastName = LastName;
            entity.Phone = Phone;
            entity.Email = Email;
            entity.CompanyId = CompanyId;
        }
    }
}
