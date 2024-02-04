using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class CreateUpdateProductDto
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
