using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StajBackend.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StajBackend.Controllers
{
    [Route("api/linq/[controller]")]
    public class UserControllerLinq : Controller
    {
        private readonly IConfiguration _configuration;
        private MongoClient dbClient;
        private IMongoDatabase db;
        private IMongoCollection<User> db_collection;
        private readonly string _apiUrl;

        public UserControllerLinq(IConfiguration configuration)
        {
            _configuration = configuration;
            dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));
            db = dbClient.GetDatabase("ArasWebAPI");
            db_collection = db.GetCollection<User>("User");
            _apiUrl = _configuration.GetConnectionString("ApiUrl");
        }

        [HttpGet]
        public JsonResult Get()
        {
            //var dbList = db_collection.AsQueryable();
            var result = from User in db_collection.AsQueryable() select User;

            return new JsonResult(result);
        }

        [HttpGet]
        [Route("/Linq/UserGetById")]
        public JsonResult UserGetById(int id)
        {

            var result =
                (from User in db_collection.AsQueryable()
                    where User.Id == id
                    select User)
                    .First();

            return new JsonResult(result);
        }

        [HttpPost]
        public JsonResult Post(User entity)
        {

            var lastUserId =
                (from User in db_collection.AsQueryable()
                 select User)
                 .Count();


            entity.Id = lastUserId + 1;

            //veri tabanına insert atıyorum 
            db_collection.InsertOne(entity);

            //Onay mesajı
            return new JsonResult("Success");
        }

        [HttpPut]
        public JsonResult Put(User entity)
        {

            // Filter ile hangi id ye ulaşmak istediğimizi bulup onun proplarını güncelleme yapıyorum
            //var filter = Builders<User>.Filter.Eq("Id", entity.Id);


            var filter = Builders<User>.Filter.Equals
                    (from User in db_collection.AsQueryable()
                     where User.Id == entity.Id
                     select User);


            //burda gerekli alanların eklenmisini yapıyorum
            var update = Builders<User>.Update.Set("name", entity.name)
                                              .Set("username", entity.username)
                                              .Set("email", entity.email)
                                              .Set("address.street", entity.address.street)
                                              .Set("address.suite", entity.address.suite)
                                              .Set("address.city", entity.address.city)
                                              .Set("address.zipcode", entity.address.zipcode)
                                              .Set("address.geo.lat", entity.address.geo.lat)
                                              .Set("address.geo.lng", entity.address.geo.lng)
                                              .Set("phone", entity.phone)
                                              .Set("website", entity.website)
                                              .Set("company.name", entity.company.name)
                                              .Set("company.catchPhrase", entity.company.catchPhrase)
                                              .Set("company.bs", entity.company.bs);


            
            //Veri tabanına ulaşıp bütün modeli tek seferde put yapıyorum
            db_collection.UpdateOne(filter, update);




            //Verilen mesaj
            return new JsonResult("Updated Successfully");
        }
    }
}

