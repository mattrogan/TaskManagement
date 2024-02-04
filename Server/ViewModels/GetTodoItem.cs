using TaskManagement.Shared.Models;

namespace Server.ViewModels;

public class GetTodoItem
{
    public GetTodoItem()
    {
    }

    public GetTodoItem(TodoItem item)
    {
        Title = item.Title;
        Description = item.Description;
        DueDate = item.DueDate;
        IsCompleted = item.IsCompleted;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }  
    public bool IsCompleted { get; set; }
}


public class CompletedTask
{
    public CompletedTask()
    {
    }

    public CompletedTask(TodoItem item)
    {
        Title = item.Title;
        Description = item.Description;
        DueDate = item.DueDate;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }  
}
