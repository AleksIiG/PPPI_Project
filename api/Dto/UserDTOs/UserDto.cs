using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Dto.UserDTOs
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public double Weight { get; set; } = 0;
        public double Height { get; set; } = 0;
        public int Age { get; set; } = 0;
        public List<string> LikedWorkouts { get; set; } = new();
        public List<string> CreatedWorkouts { get; set; } = new();
    }
}