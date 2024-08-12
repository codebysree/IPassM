using System.ComponentModel.DataAnnotations;

namespace IPassM.Entities
{
    public class UserCredentials
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public UserCredentials()
        {
        }
    }

    public class Credential
    {
        public Guid CredentialId { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Website Name is required")]
        public string WebsiteName { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
