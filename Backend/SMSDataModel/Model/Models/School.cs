namespace SMSDataModel.Model.Models
{
    public class School
    {
        public Guid Id { get; set; }
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int PinCode { get; set; }
        public int Subscription { get; set; } = 0;
        public DateOnly? SubscriptionDate { get; set; }
        public bool IsSoftDeleted { get; set; } = false;

        // ✅ Navigation property: A school can have many users
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
