using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Exceptionlogging
    {
        [Key]
        public int ID { get; set; }
        //public string Application { get; set; }
        //public DateTime Logged { get; set; }
        //public string Level { get; set; }
        public string Message { get; set; }
        //public string Logger { get; set; }
        //public string Callsite { get; set; }
        //public string Exceptions { get; set; }
    }
}
