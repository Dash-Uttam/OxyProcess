using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.Models.FormTag
{
    public class FilesManager
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string FieldName { get; set; }

        public int FieldId { get; set; }

        public int FormdataEntryId { get; set; }

        public int TemplateuniqueId { get; set; }

        public int TagId { get; set; }


        public string FileType { get; set; }

  

        
    }
}
