using Microsoft.AspNetCore.Identity;

namespace DOAN.Models
{
    public class User : IdentityUser 
    {
        // thuôicj tính user
        public bool? IsLooked { get; set; }
        public string? AccoutType { get; set; }
    }
}
