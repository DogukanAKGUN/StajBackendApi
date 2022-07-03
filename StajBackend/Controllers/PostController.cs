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

        public PostController(IConfiguration configuration)
        {
            _configuration = configuration;
            dbClient = new MongoClient(_configuration.GetConnectionString("MongoDbConnection"));

        }

        //Postları get ile alıyor
        [HttpGet]
        public JsonResult Get()
        {

            var dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("Post").AsQueryable();

            return new JsonResult(dbList);
        }
        //Verilen id ye göre collectiondan uyan id yi alıp getiriyor
        [HttpGet]
        [Route("/PostGetById")]
        public async Task<Post> PostGetById(int id)
        {

            var dbList = dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("Post");
            var item = await dbList
                             .Find(Builders<Post>.Filter.Eq("_id", id))
                             .FirstOrDefaultAsync();

            return item;
        }

        [HttpGet]
        [Route("/getdataPost")]
        public async Task<Post> FlurlGet()
        {

            var result = await "http://jsonplaceholder.typicode.com"
                .AppendPathSegment("posts")
                .SetQueryParams()
                .GetJsonAsync<IEnumerable<Post>>();

            var listPost = new Post();

            foreach (var item in result)
            {
                listPost = item;
                Post(listPost);
            }


            return null;

        }

        [HttpPost]
        public JsonResult Post(Post entity)
        {

            int lastUserId = dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("Post").AsQueryable().Count();

            entity.Id = lastUserId + 1;

            dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("Post").InsertOne(entity);

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
                                             

            dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("User").UpdateOne(filter, update);




            return new JsonResult("Updated Successfully");
        }


        [HttpDelete]
        public JsonResult Delete(int id)
        {
            var filter = Builders<Post>.Filter.Eq("Id", id);
            dbClient.GetDatabase("ArasWebAPI").GetCollection<Post>("Post").DeleteOne(filter);
            return new JsonResult("Deleted Successfully");
        }


    }
}

