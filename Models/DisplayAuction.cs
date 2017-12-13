namespace Belt.Models
{
    public class DisplayAuction : BaseEntity
    {
        public int id {get;set;}
        public string product {get;set;}
        public string description {get;set;}
        public string seller {get;set;}
        public int seller_id {get;set;}
        public decimal top_bid {get;set;}
        public string top_bidder {get;set;}
        public int top_bidder_id {get;set;}
        public string time_remaining {get;set;}
    }
}