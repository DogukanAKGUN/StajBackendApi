using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StajBackend.Model;
using Flurl;
using Flurl.Http;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StajBackend.Controllers
{
    [Route("api/post")]
    public class PostController : Controller
    {
        private readonly IConfiguration _configuration;
        private MongoClient dbClient;
        private IMongoDatabase db;
        private IMongoCollection<Post> dbCollection;
        private readonly string _apiUrl;

        public PostController(IConfiguration configuration)
        {
            _configuration = configuration;
            dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));
            db = dbClient.GetDatabase("ArasWebAPI");
            dbCollection = db.GetCollection<Post>("Post");
            _apiUrl = _configuration.GetConnectionString("ApiUrl");
        }

        //Postları get ile alıyor
        [HttpGet]
        public JsonResult Get()
        {

            var dbList = dbCollection.AsQueryable();

            return new JsonResult(dbList);
        }
        //Verilen id ye göre collectiondan uyan id yi alıp getiriyor
        [HttpGet]
        [Route("/PostGetById")]
        public async Task<Post> PostGetById(int id)
        {

            var dbList = dbCollection;
            var item = await dbList
                             .Find(Builders<Post>.Filter.Eq("_id", id))
                             .FirstOrDefaultAsync();

            return item;
        }

        [HttpGet]
        [Route("/getdataPost")]
        public async Task<IActionResult> FlurlGet()
        {

            var result = await _apiUrl
                            .AppendPathSegment("posts")
                            .SetQueryParams()
                            .GetJsonAsync<IEnumerable<Post>>();

           

            foreach (var item in result)
            {
                
                Post(item);
            }


            return Ok();

        }

        [HttpPost]
        public JsonResult Post(Post entity)
        {

            int lastUserId = dbCollection.AsQueryable().Count();

            entity.Id = lastUserId + 1;

            dbCollection.InsertOne(entity);

            return new JsonResult("Success");
        }

        //Put ile var olan bir kaydı güncelledim
        [HttpPut]
        public JsonResult Put(Post entity)
        {
            var filter = Builders<Post>.Filter.Eq("Id", entity.Id);
            var update = Builders<Post>.Update.Set("userId", entity.userId)
                                              .Set("title", entity.title)
                                              .Set("body", entity.body);


            dbCollection.UpdateOne(filter, update);




            return new JsonResult("Updated Successfully");
        }


        [HttpDelete]
        public JsonResult Delete(int id)
        {
            var filter = Builders<Post>.Filter.Eq("Id", id);
            dbCollection.DeleteOne(filter);
            return new JsonResult("Deleted Successfully");
        }


    }
}

