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

        public static UserDto? ToUserDto(this AppUser appUser)
        {
            if (appUser.Id == null) return null;
            return new UserDto
            {
                Id = appUser.Id,
                Email = appUser.Email,
                UserName = appUser.Username,
                Role = appUser.Role
            };
        }
    }

}