using Amazon.api.Responses;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Infrastructure.DTOs;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Amazon.api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityServices _securityServices;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        public SecurityController(ISecurityServices securityServices,
            IMapper mapper,
            IPasswordService passwordService)
        {
            _securityServices = securityServices;
            _mapper = mapper;
            _passwordService = passwordService;
        }

        //[Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.User)}")]
        [HttpPost("register")]
        public async Task<IActionResult> Order(SecurityDto securityDto)
        {
            var security = _mapper.Map<Security>(securityDto);
            security.Password = _passwordService.Hash(securityDto.Password);

            await _securityServices.RegisterUser(security);

            securityDto = _mapper.Map<SecurityDto>(security);
            var response = new ApiResponse<SecurityDto>(securityDto);
            return Ok(response);
        }


    }
}
