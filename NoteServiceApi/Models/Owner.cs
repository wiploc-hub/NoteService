namespace NoteServiceApi.Models
{
    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }

        public bool Active { get; set; }
    }
}
