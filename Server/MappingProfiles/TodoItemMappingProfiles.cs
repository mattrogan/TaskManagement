using AutoMapper;
using TaskManagement.Server.ViewModels;
using TaskManagement.Shared.Models;

namespace TaskManagement.Server.MappingProfiles
{
    public class TodoItemMappingProfiles : Profile
    {
        public TodoItemMappingProfiles()
        {
            CreateMap<PostTodoItem, TodoItem>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ForMember(dest => dest.Id, opts => opts.Ignore())
                .ForMember(dest => dest.Title, opts => opts.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.DueDate, opts => opts.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.IsCompleted, opts => opts.Ignore());
        }
    }
}
