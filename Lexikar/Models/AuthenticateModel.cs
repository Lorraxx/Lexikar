using System.ComponentModel.DataAnnotations;

namespace Lexikar.Models
{
    public class AuthenticateModel // Duplicite
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }    // Do not store password like this!
    }
}
