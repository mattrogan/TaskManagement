namespace Server.ViewModels;

public class GetTodoItem
{
    public GetTodoItem()
    {
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

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }  
}
