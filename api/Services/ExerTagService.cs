using api.Dto;
using api.Interface;
using api.Models;
using api.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public class ExerTagService: IExerTagService
    {
        private readonly IExerTagRepository _exerTagRepo;

        public ExerTagService(IExerTagRepository exerTagRepo)
        {
            _exerTagRepo = exerTagRepo;
        }
    }
}
