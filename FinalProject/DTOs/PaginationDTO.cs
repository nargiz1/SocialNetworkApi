using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class PaginationDTO
    {
        public string UserId { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}
