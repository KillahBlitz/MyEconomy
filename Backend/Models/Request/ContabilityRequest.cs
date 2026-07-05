using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Resquest
{
    public class ContabilityRequest
    {
        [Required]
        public short UserId { get; set; }

        [Required]
        public DateOnly startDate { get; set; }

        [Required]
        public DateOnly endDate { get; set; }

        public bool neededSaldo { get; set; } = true;

        public List<int> categories { get; set; }
    }
}
