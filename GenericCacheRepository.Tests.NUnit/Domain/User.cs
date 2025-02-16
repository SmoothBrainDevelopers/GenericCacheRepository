using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GenericCacheRepository.Tests.NUnit.Domain
{
    [PrimaryKey(nameof(Id))]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
    }
}
