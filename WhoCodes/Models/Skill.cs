using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WhoCodes.Models
{
    public class Skill
    {
        public Skill()
        {
            Contacts = new List<ContactSkill>();
        }

        public int Id { get; set; }

        [StringLength(64), Required]
        public string Name { get; set; }

        public IList<ContactSkill> Contacts { get; set; }
    }
}
