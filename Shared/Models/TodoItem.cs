using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace TaskManagement.Shared.Models
{
    public class TodoItem
    {
        public TodoItem()
        {
        }

        public TodoItem(string title, string desc, DateTime dueDate)
            : this()
        {
            Title = title;
            Description = desc;
            DueDate = dueDate;
        }

        [Required]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public bool IsCompleted { get; set; } = false;

        public override bool Equals(object? obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            TodoItem? other = obj as TodoItem;
            if (other is null)
            {
                return false;
            }

            return this.Id == other.Id
                && this.Title == other.Title
                && this.Description == other.Description
                && this.DueDate == other.DueDate
                && this.IsCompleted == other.IsCompleted;
        }
    }
}
