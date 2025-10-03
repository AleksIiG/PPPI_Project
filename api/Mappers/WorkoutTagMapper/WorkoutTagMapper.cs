using api.Dto.ExerTagsDto;
using api.Dto.WorkoutTagDto;
using api.Models;
using System.Runtime.CompilerServices;

namespace api.Mappers.WorkoutMapper
{
    public static class WorkoutTagMapper
    {
        public static WorkoutTagDto? ToWorkoutTagDto(this WorkoutTag workTag)
        {
            return new WorkoutTagDto
            {
                Id = workTag.Id,
                Name = workTag.Name,
            };
        }

        public static WorkoutTag ToWorkoutTagFromCreateDto(this CreateWorkoutTagDto createdWorkTag)
        {
            return new WorkoutTag
            {
                Name = createdWorkTag.Name
            };
        }

        public static WorkoutTag ToWorkoutTagFromUpdateDto(this UpdateWorkoutTagDto updatedWorkTag)
        {
            return new WorkoutTag
            {
                Name = updatedWorkTag.Name
            };
        }
    }
}
