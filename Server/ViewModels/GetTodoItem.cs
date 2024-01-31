using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManagement.Shared.Models;

namespace Server.ViewModels;

public class GetTodoItem
{
    public GetTodoItem(TodoItem task)
    {
        Title = task.Title;
        Description = task.Description;
        DueDate = task.DueDate;
        IsCompleted = task.IsCompleted;
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

    public CompletedTask(TodoItem task)
        : this()
    {
        Title = task.Title;
        Description = task.Description;
        DueDate = task.DueDate;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }  
}