using api.Dto.ExerciseDTOs;
using api.Dto.WorkoutTagDtos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace api.Dto.WorkoutDto
{
    public class WorkoutDto
    {
        public string? Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<ExerciseDto> Exercise { get; set; } = new();

        public string UserId { get; set; } = string.Empty;

        public List<WorkoutTagDto> Tags { get; set; } = new();
    }
}
