using api.Dto.ExerciseDTOs;
using api.Dto.WorkoutDto;
using api.Dto.WorkoutTagDtos;
using api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Mappers.WorkoutMapper
{
    public static class WorkoutMapper
    {
        public static WorkoutDto ToWorkoutDto(Workout workout, List<ExerciseDto> exercises, List<WorkoutTagDto> tags)
        {
            return new WorkoutDto
            {
                Id = workout.Id,
                Name = workout.Name,
                Description = workout.Description,
                Exercise = exercises,
                UserId = workout.UserId,
                Tags = tags
            };
        }

        public static Workout ToWorkoutFromCreateDto(this CreateWorkoutDto created)
        {
            return new Workout
            {
                Name = created.Name,
                Description = created.Description,
                ExerciseIds = created.ExerciseIds ?? new List<string>(),
                UserId = created.UserId,
                TagsIds = created.TagsIds ?? new List<string>(),
            };
        }

        public static Workout ToWorkoutFromUpdateDto(this UpdateWorkoutDto updated)
        {
            return new Workout
            {
                Name = updated.Name,
                Description = updated.Description,
                ExerciseIds = updated.ExerciseIds ?? new List<string>(),
                UserId = updated.UserId,
                TagsIds = updated.TagsIds ?? new List<string>(),
            };
        }
    }
}
