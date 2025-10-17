
using api.Dto.ExerciseDTOs;
using api.Dto.ExerTagsDto;
using api.Helpers;
using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace api.Services.Interfaces
{
        public interface IExerciseService
        {

                Task<List<Exercise>> GetAllAsync(QueryObjectForExercises query);

                Task<Exercise> GetByIdAsync(string id);
                Task<Exercise> CreateAsync(Exercise exercise);
                Task<Exercise> UpdateAsync(string id, Exercise exercise);
                Task<Exercise?> DeleteAsync(string id);
                Task<List<ExerciseDto>> GetByIdsFromWorkoutsAsync(IEnumerable<string> exerciseIds);

        }
}