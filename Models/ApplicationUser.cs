using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ChatAPI.Models
{
    public class ApplicationUser: IdentityUser
    {
        [MaxLength(50)]
        public string? Fname {  get; set; }
        public ICollection<Conversation> Conversations { get; set; }
    }
}
