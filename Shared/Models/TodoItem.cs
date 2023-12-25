using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace TaskManagement.Shared.Models
{
    [DataContract]
    public class TodoItem
    {
        [DataMember(Name = "id")]
        [Required]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [DataMember(Name = "desc")]
        [Required]
        public string Description { get; set; } = string.Empty;

        [DataMember(Name = "due")]
        [Required]
        public DateTime DueDate { get; set; }
    }
}
