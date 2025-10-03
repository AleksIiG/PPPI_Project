using api.Data;
using api.Dto.WorkoutTagDto;
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
    public class WorkoutTagRepository: IWorkoutTagRepository
    {
        private readonly MongoDbService _database;

        public WorkoutTagRepository(MongoDbService database) 
        {
            _database = database;
        }

        public async Task<List<WorkoutTag>> GetByIdsFromWorkoutsAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var objectIds = tagIdsList.Select(id => ObjectId.Parse(id)).ToList();
            var filter = Builders<WorkoutTag>.Filter.In("_id", objectIds);

            return await _database.WorkoutTags
                .Find(filter)
                .ToListAsync();
        }

        public async Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds)
        {
            var tagIdsList = tagIds.ToList();
            var objectIds = tagIds.Select(id => ObjectId.Parse(id)).ToList();
            var filter = Builders<WorkoutTag>.Filter.In("_id", objectIds);

            return await _database.WorkoutTags
                .Find(filter)
                .Project(t => t.Id.ToString())
                .ToListAsync();
        }

        public async Task<List<WorkoutTag>> GetAllAsync()
        {
            return await _database.WorkoutTags.Find(_ => true).ToListAsync();
        }

        public async Task<WorkoutTag?> GetByIdAsync(string id)
        {
            return await _database.WorkoutTags.Find(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<WorkoutTag> CreateAsync(WorkoutTag workoutTag)
        {
            await _database.WorkoutTags.InsertOneAsync(workoutTag);
            return workoutTag;
        }

        public async Task<WorkoutTag> UpdateAsync(WorkoutTag workoutModel, string id)
        {
            var updated = await _database.WorkoutTags.ReplaceOneAsync(t => t.Id == id, workoutModel);
            if (updated == null)
            {
                return null;
            }
            return workoutModel;
        }

        public async Task<WorkoutTag> DeleteAsync(string id)
        {
            var tag = await _database.WorkoutTags.Find(t => t.Id == id).FirstOrDefaultAsync();
            if (tag == null)
            {
                return null;
            }
            await _database.WorkoutTags.DeleteOneAsync(t => t.Id == id);
            return tag;
        }
    }
}
