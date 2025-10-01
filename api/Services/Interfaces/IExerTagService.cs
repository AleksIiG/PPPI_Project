using api.Dto;
using api.Dto.ExerTagsDto;
using api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services.Interfaces
{
    public interface IExerTagService
    {
        Task<List<ExerTagDto>> GetByIdsFromExercisesAsync(IEnumerable<string> tagIds);

        Task<List<string>> GetNonExistingTagsAsync(IEnumerable<string> tagIds);
    }
}
