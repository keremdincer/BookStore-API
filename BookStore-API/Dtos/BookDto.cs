using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? Year { get; set; }
        public string ISBN { get; set; }
        public string Summary { get; set; }
        public string Image { get; set; }
        public double? Price { get; set; }

        public int AuthorId { get; set; }
        public virtual AuthorDto Author { get; set; }
    }

    public class BookCreateDto
    {
        [Required]
        public string Title { get; set; }

        public int? Year { get; set; }

        [Required]
        public string ISBN { get; set; }

        [StringLength(500)]
        public string Summary { get; set; }

        public string Image { get; set; }

        public double? Price { get; set; }

        [Required]
        public int AuthorId { get; set; }
    }

    public class BookUpdateDto
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public int? Year { get; set; }

        [StringLength(500)]
        public string Summary { get; set; }

        public string Image { get; set; }

        public double? Price { get; set; }

        [Required]
        public int AuthorId { get; set; }
    }
}
