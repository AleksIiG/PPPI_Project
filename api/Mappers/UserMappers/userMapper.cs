using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
using api.Dto.UserDTOs;
using api.Models;

namespace api.Mappers.UserMapper
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

        public static AppUser ToAppUserFromRegisterDto(this RegisterUserDto registerUserDto)
        {
            return new AppUser
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.PasswordHash)
            };
        }
    }
    
}