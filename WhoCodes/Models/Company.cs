using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhoCodes.Models
{
    public class Company
    {
        public Company()
        {
            Contacts = new List<Contact>();
        }

        public int Id { get; set; }

        [StringLength(128), Required]
        public string Name { get; set; }

        [StringLength(128)]
        public string Website { get; set; }

        public IList<Contact> Contacts { get; set; }
    }
}
