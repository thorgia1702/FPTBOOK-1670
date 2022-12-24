using System.ComponentModel.DataAnnotations;

namespace ASM2.Models
{
    public class Orders
    {
        public int Id { get; set; }

        
        [Display(Name = "Email")]
        public string Email { get; set; }

   
        [Display(Name = "CartItem")]
        public string CartItem { get; set; }

        public double? Total { get; set; }
        public string? Phone { get; set; }
        public DateTime? Orderdate { get; set; }
      }
}
