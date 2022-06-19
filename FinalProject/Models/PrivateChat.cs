using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class PrivateChat : Base
    {
        public string UserOneId { get; set; }
        public ApiUser UserOne { get; set; }
        public string UserTwoId { get; set; }
        public ApiUser UserTwo { get; set; }
    }
}
