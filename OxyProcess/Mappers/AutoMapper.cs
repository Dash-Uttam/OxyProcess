using OxyProcess.Models.Group;
using OxyProcess.Models.GroupsViewModels;
using OxyProcess.Models.Template;
using OxyProcess.ViewModels.Template;

namespace OxyProcess.Mappers
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<GroupViewModel, Groups>();
            CreateMap<Groups,GroupViewModel>();

            CreateMap<TemplateViewModel, Template>();
            CreateMap<Template,TemplateViewModel>();


        }
    }
}
