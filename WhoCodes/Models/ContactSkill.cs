namespace WhoCodes.Models
{
    public class ContactSkill
    {
        public int ContactId { get; set; }

        public Contact Contact { get; set; }

        public int SkillId { get; set; }

        public Skill Skill { get; set; }
    }
}
