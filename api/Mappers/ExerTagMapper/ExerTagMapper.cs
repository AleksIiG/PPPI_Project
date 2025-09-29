using api.Dto.ExerTagsDto;
using api.Models;
using System.Runtime.CompilerServices;



namespace api.Mappers.ExerTagMapper
{
    public static class ExerTagMapper
    {
        public static ExerTagDto? ToExerTagDto(this ExerciseTag exTag)
        {
            return new ExerTagDto
            {
                Id = exTag.Id,
                Name = exTag.Name,
            };
        }

        public static ExerciseTag ToExerTagFromCreateDto(this CreateExerTagDto createdExerTag) 
        {
            return new ExerciseTag
            {
                Name = createdExerTag.Name
            };
        }

        public static ExerciseTag ToExerTagFromUpdateDto(this UpdateExerTagDto updatedExerTag)
        {
            return new ExerciseTag
            {
                Name = updatedExerTag.Name
            };
        }
    }
}
