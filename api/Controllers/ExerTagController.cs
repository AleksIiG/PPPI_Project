using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dto.ExerTagsDto;
using api.Interface;
using api.Mappers;
using api.Mappers.ExerTagMapper;
using api.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;



namespace api.Controllers
{
    [Route("api/exercisesTags")]
    [ApiController]
    public class ExerTagController: ControllerBase
    {
        private readonly IExerTagService _exerTagService;

        public ExerTagController(IExerTagService exerTagService)
        {
            _exerTagService = exerTagService;
        }



    }
}
