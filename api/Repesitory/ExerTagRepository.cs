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

        public async Task<List<ExerciseTag>> GetByIdsFromExercisesAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var objectIds = tagIdsList.Select(id => ObjectId.Parse(id)).ToList();
            var filter = Builders<ExerciseTag>.Filter.In("_id", objectIds);

            return await _database.ExerciseTags
                .Find(filter)
                .ToListAsync();
        }

        public async Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var objectIds = tagIds.Select(id => ObjectId.Parse(id)).ToList();
            var filter = Builders<ExerciseTag>.Filter.In("_id", objectIds);

            return await _database.ExerciseTags
                .Find(filter)
                .Project(t => t.Id.ToString())
                .ToListAsync();
        }
    }
}
