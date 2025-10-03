using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerciseDTOs;
using api.Dto.UserDTOs;
using api.Models;

namespace api.Mappers.UserMapper
{
    public static class UserMapper
    {
        public static AppUser ToAppUserFromRegisterDto(this RegisterUserDto registerUserDto)
        {
            return new AppUser
            {
                Id = registerUserDto.Id,
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password)
            };
        }

        public static AppUser ToAppUserFromLoginDto(this LoginUserDto loginUserDto)
        {
            return new AppUser
            {
                Email = loginUserDto.Email,
                PasswordHash = loginUserDto.Password
            };
        }
    }
    
}