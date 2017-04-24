namespace DataGridSample
{
    public class Guest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }

        public Guest(string firstName, string lastName, Gender gender)
        {
            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
        }
    }
}