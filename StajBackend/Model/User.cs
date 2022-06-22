using System;
using MongoDB.Bson;

namespace StajBackend.Model
{
    public class User
    {
        public int Id { get; set; }

        public string name { get; set; }

        public string username { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public string website { get; set; }

        public Address address { get; set; }

        public Company company { get; set; }

        public List<Bank> userBankAccounts { get; set; }
    }
}
