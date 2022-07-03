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
        
        //TODO: FAVORİLERE EKLEME SERVİSİ YAZILACAK VERİTABANI KISMI HALLEDİLDİ

        private readonly IConfiguration _configuration;
        private MongoClient dbClient;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));

        }
        
       

        //veritabanından bütün verileri alıyorum ne olduğu fark etmeksizin

        [HttpGet]
        public JsonResult Get()
        {
            var dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").AsQueryable();

            return new JsonResult(dbList);
        }

        //Verilen id ye göre collectiondan uyan id yi alıp getiriyor
        [HttpGet]
        [Route("/UserGetById")]
        public async Task<User> UserGetById(int id)
        {

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
        public JsonResult Put(User entity , Bank array)
        {
           
            // Filter ile hangi id ye ulaşmak istediğimizi bulup onun proplarını güncelleme yapıyorum
            var filter = Builders<User>.Filter.Eq("Id", entity.Id);
            var arrayfilter = Builders<Bank>.Filter.Eq("Id", array.id);

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
            //Sıkıntılı!!!!
            var arrayUpdate = Builders<Bank>.Update.Set("bankName", array.bankName)
                                                   .Set("bankNumber", array.bankNumber);
                                              
            
            //Veri tabanına ulaşıp bütün modeli tek seferde put yapıyorum
            dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").UpdateOne(filter, update);

            dbClient.GetDatabase("ArasWebAPI").GetCollection<Bank>("User.userBankAccounts").UpdateOne(arrayfilter, arrayUpdate);


            //Verilen mesaj
            return new JsonResult("Updated Successfully");
        }

        [HttpDelete]
        public JsonResult Delete(int id)
        {
            
            var filter = Builders<User>.Filter.Eq("Id", id);
            dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").DeleteOne(filter);
            return new JsonResult("Deleted Successfully");
        }

        /*[HttpGet]
        [Route("/getfavorite")]
        public JsonResult GetFavorite(User entity)
        {
            var dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").AsQueryable();
            var filter = Builders<User>.Filter.Eq("Id", entity.Id);

            return null;
        }*/

        //CAST PROBLEMİ VAR
        [HttpPost]
        [Route("/addfavorite")]
        public JsonResult AddFavorite(int u_id , Favorites fav)
        {

            var collection = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User");
            var filter = Builders<User>.Filter.Eq("Id", u_id);
            var update = Builders<User>.Update.AddToSet("favorites",fav);
            var result = collection.UpdateOne(filter, update);



            return new JsonResult(result);
        }

        [HttpDelete]
        [Route("/deletefavorite")]
        public JsonResult DeleteFavorite(int u_id,int p_id)
        {
            var collection = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User");

            var filter = Builders<User>.Filter.Eq("Id", u_id);
            var update = Builders<User>.Update.PullFilter(y => y.favorites, builder => builder.postId == p_id);
            var result = collection.UpdateOne(filter, update);
            return new JsonResult("Deleted succsesfully");
        }

        
    }
}




/*var u_dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User");
            var u_item =  u_dbList
                             .Find(Builders<User>.Filter.Eq("_id", u_id))
                             .FirstOrDefault();

            var list = u_item.favorites;

            var p_dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("Post");
            var p_item = p_dbList
                             .Find(Builders<Post>.Filter.Eq("_id", p_id))
                             .FirstOrDefault();


            */

//TODO:Ekleme Yapılacak
//var a = Builders<User>.Update.AddToSet("favorites", p_id);



/*var filter = Builders<User>.Filter.And(
Builders<User>.Filter.Eq("Id", u_id)
);
var update = Builders<User>.Update.AddToSet("favorites", lastFavoriteId);

dbClient.GetDatabase("ArasWebAPI").GetCollection<User>("User").FindOneAndUpdateAsync(filter, update);
*/