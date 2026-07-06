using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Resquest
{
    public class DebsRequest
    {
        [Required]
        public short UserId { get; set; }

        [Required]
        public DateOnly startDate { get; set; }

        [Required]
        public DateOnly endDate { get; set; }

    }
}
