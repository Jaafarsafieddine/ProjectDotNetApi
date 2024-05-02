using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
namespace ProjectDotNet.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int phoneNumber { get; set; }

    }
}
