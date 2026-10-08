namespace NoteServiceApi.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;


        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }

        public bool Active { get; set; }

    }
}
