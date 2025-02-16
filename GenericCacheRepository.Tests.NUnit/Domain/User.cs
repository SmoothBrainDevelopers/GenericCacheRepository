using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GenericCacheRepository.Tests.NUnit.Domain
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
    }
}
