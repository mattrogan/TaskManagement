namespace Server.ViewModels;

/// <summary>
/// Model for posting which tasks to complete
/// </summary>
public class CompleteTodoItem
{
    public int Id { get; set; }
    public bool Complete { get; set; }
}