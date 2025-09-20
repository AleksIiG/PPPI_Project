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

        public static Exercise ToExerciseFromExerciseDto(this ExerciseDto exerciseDto)
        {
            return new Exercise
            {
                Id = exerciseDto.Id,
                Name = exerciseDto.Name,
                Description = exerciseDto.Description,
                TagsIds = exerciseDto.TagsIds ?? new List<string>()
            };
        }
    }
}