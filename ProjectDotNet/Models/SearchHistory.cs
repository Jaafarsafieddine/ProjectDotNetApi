using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class SearchHistory
    {
        [Key]
        public int Id { get; set; }
        public int userId { get; set; }
        public User user { get; set; }
        public List<string> searchHistory { get; set; }
    }
}
