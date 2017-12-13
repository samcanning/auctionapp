using System.ComponentModel.DataAnnotations;
namespace Belt.Models
{
    public class LoginValidator : BaseEntity
    {
        [Required(ErrorMessage = "Username is required.")]
        public string log_username {get;set;}
        [Required(ErrorMessage = "Password is requied.")]
        [DataType(DataType.Password, ErrorMessage = "Invalid password format.")]
        public string log_pw {get;set;}
    }
}