using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericCacheRepository.Test.MS
{
    [PrimaryKey(nameof(Id))]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
