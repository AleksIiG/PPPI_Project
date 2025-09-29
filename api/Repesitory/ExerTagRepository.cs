using api.Data;
using api.Dto.ExerciseDTOs;
using api.Interface;
using api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Repesitory
{
    public class ExerTagRepository: IExerTagRepository
    {
        private readonly MongoDbService _database;

        public ExerTagRepository(MongoDbService database)
        {
            _database = database;
        }
    }
}
