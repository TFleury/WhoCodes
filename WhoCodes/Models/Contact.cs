using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhoCodes.Models
{
    public class Contact
    {
        public Contact()
        {
            Skills = new List<ContactSkill>();
        }

        public int Id { get; set; }

        [StringLength(128), Required]
        public string FirstName { get; set; }

        [StringLength(128), Required]
        public string LastName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(256), Required]
        public string Email { get; set; }

        public int? CompanyId { get; set; }

        public Company Company { get; set; }

        public IList<ContactSkill> Skills { get; set; }
    }
}
