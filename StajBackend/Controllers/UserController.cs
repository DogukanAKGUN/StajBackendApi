﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Text.Json;
using StajBackend.Model;
using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;

namespace StajAPI.Controllers
{
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        
       

        private readonly IConfiguration _configuration;
        private MongoClient dbClient;
        private IMongoDatabase db;
        private IMongoCollection<User> db_collection;
        private readonly string _apiUrl;

        private UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));
            db = dbClient.GetDatabase("ArasWebAPI");
            db_collection = db.GetCollection<User>("User");
            _apiUrl = _configuration.GetConnectionString("ApiUrl");
            
        }
        
        [HttpGet]
        [Route("/string")]
        public JsonResult conn()
        {
            var a = _configuration.GetConnectionString("MongoDbConnection");
            return new JsonResult(a);
        }

        //veritabanından bütün verileri alıyorum ne olduğu fark etmeksizin

        [HttpGet]
        public JsonResult Get()
        {
            var dbList = db_collection.AsQueryable();

            return new JsonResult(dbList);
        }


        //TODO unhandled exepction ve handled exepction araştır
        //Verilen id ye göre collectiondan uyan id yi alıp getiriyor
        [HttpGet]
        [Route("/UserGetById")]
        public JsonResult UserGetById(int id)
        {

            var dbList = db_collection;
            var item =  dbList
                             .Find(Builders<User>.Filter.Eq("_id", id))
                             .FirstOrDefault();

            return new JsonResult(item);
        }
        
        //özel bir route verdim çünkü her seferinde erişilmesini istemiyorum
        [HttpGet]
        [Route("/GetDataUser")]
        public async Task<IActionResult> FlurlGet()
        {
            //flurl kütüphanesinden yararlanıp verilen Api ye request atıp verileri alıyorum
            var result = await _apiUrl
                    .AppendPathSegment("users")
                    .SetQueryParams()
                    .GetJsonAsync<IEnumerable<User>>();            
            
            foreach (var item in result)
            {
                
                Post(item);
                
            }

            //ne döndüreceğime karar vermediğim için şimdilik null döndürüyorum

            return Ok();
        }


       
        //post ile 1 adet user ekleme yapılıyor her seferinde bulunan id'nin bir sonraki ekleniyor
        [HttpPost]
        public JsonResult Post(User entity)
        {
          
            //en son userın idsini buluyorum ve bir sonraki id ye ekleme yapıyorum 
            int lastUserId = db_collection.AsQueryable().Count();
            entity.Id = lastUserId + 1;

            //veri tabanına insert atıyorum 
            db_collection.InsertOne(entity);

            //Onay mesajı
            return new JsonResult("Success");
        }


        //Put ile var olan bir kaydı güncelledim
        //eksikliği array içine kayıt yapamıyorum
        [HttpPut]
        public JsonResult Put(User entity)
        {
           
            // Filter ile hangi id ye ulaşmak istediğimizi bulup onun proplarını güncelleme yapıyorum
            var filter = Builders<User>.Filter.Eq("Id", entity.Id);
            

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

        [HttpDelete]
        public JsonResult Delete(int id)
        {
            
            var filter = Builders<User>.Filter.Eq("Id", id);
            db_collection.DeleteOne(filter);
            return new JsonResult("Deleted Successfully");
        }


        [HttpPost]
        [Route("/addfavorite")]
        public JsonResult AddFavorite(int u_id , Favorites fav)
        {

            var collection = db_collection;
            //hangi kullanıcıya ekleme yapmak istediğimi buluyorum
            var filter = Builders<User>.Filter.Eq("Id", u_id);
            //burada bulduğum kullanıcının favorilerine ekleme yapmak için bilgileri ekliyor 
            var update = Builders<User>.Update.AddToSet("favorites",fav);
            //burada ekleme işlemini gerçekleştiriyorum
            var result = collection.UpdateOne(filter, update);



            return new JsonResult(result);
        }

        [HttpDelete]
        [Route("/deletefavorite")]
        public JsonResult DeleteFavorite(int u_id,int p_id)
        {
            var collection = db_collection;
            //hangi kullanıcıdan silmemiz gerektiğini filtre ediyorum
            var filter = Builders<User>.Filter.Eq("Id", u_id);
            //burada favoriler arrayinden bulduğum uygun post id olanı buluyorum
            var update = Builders<User>.Update.PullFilter(y => y.favorites, builder => builder.postId == p_id);
            //silme işlemini gerçekleştiriyorum
            var result = collection.UpdateOne(filter, update);
            return new JsonResult("Deleted successfully");
        }

        
    }
}
