using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OxyProcess.ViewModels.FormTag
{
    public class TagInsideTemplatesJson
    {
        [Key]
        public int Id { get; set; }
        public string name { get; set; }
        public string secondname { get; set; }
        public int temptype { get; set; }
        public string group { get; set; }

        public int id { get; set; }
        public int uniqid { get; set; }

        public string type { get; set; }

        public List<child> children = new List<child>();

       


    }


    public class child
    {
        public string name { get; set; }
        public string secondname { get; set; }
        public int id { get; set; }

        public int temptype { get; set; }
        public int group { get; set; }

        public int uniqid { get; set; }

        public string type { get; set; }




    }


}
