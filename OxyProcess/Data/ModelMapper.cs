using AutoMapper;
using OxyProcess.Models.FormTag;
using OxyProcess.ViewModels.FormTag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Data
{
    public class ModelMapper:Profile
    {

        public ModelMapper()
        {
            CreateMap<TemplateFields, TemplateFieldsViewModel>();
            CreateMap<TemplateFieldsViewModel, TemplateFields>();
            CreateMap<TaginsideTemplates,TaginsidTemplatesViewModel>();
        }
    }
}
