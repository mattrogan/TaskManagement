using System.Runtime.Serialization;

namespace TaskManagement.Server.ViewModels
{
    public class PostTodoItem
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime DueDate { get; set; }
    }
}
