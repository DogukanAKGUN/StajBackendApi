using System;
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
        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //veritabanından bütün verileri alıyorum ne olduğu fark etmeksizin
        
        [HttpGet]
        public JsonResult Get()
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));

            var dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").AsQueryable();

            return new JsonResult(dbList);
        }

        //Verilen id ye göre collectiondan uyan id yi alıp getiriyor
        [HttpGet]
        [Route("/UserGetById")]
        public async Task<User> GetId(int id)
        {
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));

            var dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User");
            var item = await dbList
                             .Find(Builders<User>.Filter.Eq("_id", id))
                             .FirstOrDefaultAsync();

            return item;
        }
        
        //özel bir route verdim çünkü her seferinde erişilmesini istemiyorum
        [HttpGet]
        [Route("/getdataUser")]
        public async Task<User> FlurlGet()
        {
            //flurl kütüphanesinden yararlanıp verilen Api ye request atıp verileri alıyorum
            var result = await "http://jsonplaceholder.typicode.com"
                    .AppendPathSegment("users")
                    .SetQueryParams()
                    .GetJsonAsync<IEnumerable<User>>();

            //user tipinde bir değişken tanımlayıp aldığım verilerin modele uymasını sağlıyorum ve foreach kullanarak her bir verinin kayır etme fonksiyonuna gönderip veritabanına kaydını sağlıyorum 
            var listItem = new User();
            
            foreach (var item in result)
            {
                listItem = item;
                Post(listItem);
                
            }

            //ne döndüreceğime karar vermediğim için şimdilik null döndürüyorum

            return null;
        }


       
        //post ile 1 adet user ekleme yapılıyor her seferinde bulunan id'nin bir sonraki ekleniyor
        [HttpPost]
        public JsonResult Post(User entity)
        {
            //veri tabanı bağlantısı
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));
            //en son userın idsini buluyorum ve bir sonraki id ye ekleme yapıyorum 
            int lastUserId = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").AsQueryable().Count();
            entity.Id = lastUserId + 1;

            //veri tabanına insert atıyorum 
            dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").InsertOne(entity);

            //Onay mesajı
            return new JsonResult("Success");
        }


        //Put ile var olan bir kaydı güncelledim
        //eksikliği array içine kayıt yapamıyorum
        [HttpPut]
        public JsonResult Put(User entity)
        {
           
            MongoClient dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));
            // Filter ile hangi id ye ulaşmak istediğimizi bulup onun proplarını güncelleme yapıyorum
            var filter = Builders<User>.Filter.Eq("Id", entity.Id);
            //burda gerekli alanların eklenmisini yapıyorum
            var update = Builders<User>.Update.Set("Name", entity.name)
                                              .Set("username",entity.username)
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
            dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").UpdateOne(filter, update);

            //Verilen mesaj
            return new JsonResult("Updated Successfully");
        }
    }
}
