﻿using System.ComponentModel.DataAnnotations;

namespace ProjectDotNet.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string categoryName { get; set; }
        public string parentCategory { get; set; }
        public ICollection<Car> Cars { get; set; }
    }
}
