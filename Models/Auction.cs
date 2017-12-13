using System;
namespace Belt.Models
{
    public class Auction : BaseEntity
    {
        public int id {get;set;}
        public string product {get;set;}
        public string description {get;set;}
        public decimal top_bid {get;set;}
        public DateTime end_date {get;set;}
        public int creator_id {get;set;}
        public int top_bidder_id {get;set;}
    }
}