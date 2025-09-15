using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Driver;

namespace api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly MongoDbService _mongo;
        public UserController(MongoDbService mongo)
        {
            _mongo = mongo;
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _mongo.Users.Find(_ => true).ToList();
            return Ok(users);
        }
    }
}