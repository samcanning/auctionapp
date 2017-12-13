using System.ComponentModel.DataAnnotations;
namespace Belt.Models
{
    public class RegValidator : BaseEntity
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(20, ErrorMessage = "Username cannot be longer than 20 characters.")]
        public string username {get;set;}
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
        public string password {get;set;}
        [Required(ErrorMessage = "You must confirm your password.")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
        [Compare("password", ErrorMessage = "Passwords must match.")]
        public string pw_confirm {get;set;}
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid first name.")]
        public string first_name {get;set;}
        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Invalid last name.")]
        public string last_name {get;set;}
    }
}