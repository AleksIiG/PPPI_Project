using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
using api.Models;

namespace api.Mappers.ExerciseMapper
{
    public static class ExerciseMapper
    {
        public static ExerciseDto? ToExerciseDto(this Exercise exercise)
        {
            return new ExerciseDto
            {
                Id = exercise.Id ?? string.Empty,
                Name = exercise.Name,
                Description = exercise.Description,
                TagsIds = exercise.TagsIds ?? new List<string>()
            };
        }

        public static Exercise ToExerciseFromCreateExerciseDto(this CreateExerciseDto createExerciseDto)
        {
            return new Exercise
            {
                Name = createExerciseDto.Name,
                Description = createExerciseDto.Description,
                TagsIds = createExerciseDto.TagsIds ?? new List<string>()
            };
        }
    }
}