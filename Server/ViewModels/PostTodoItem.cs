using System.Runtime.Serialization;

namespace TaskManagement.Server.ViewModels
{
    [DataContract]
    public class PostTodoItem
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "desc")]
        public string Description { get; set; }

        [DataMember(Name = "due")]
        public DateTime DueDate { get; set; }
    }
}
