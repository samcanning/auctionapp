namespace Belt.Models
{
    public class BaseEntity {}
    
    public class User : BaseEntity
    {
        public int id {get;set;}
        public string username {get;set;}
        public string first_name {get;set;}
        public string last_name {get;set;}
        public string password {get;set;}
        public decimal wallet {get;set;}
    }
}