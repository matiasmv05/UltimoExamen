using Amazon.api.Responses;
using Amazon.Core.Entities;
using Amazon.Core.Interface;
using Amazon.Infrastructure.DTOs;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <remarks>
        /// Este endpoint permite registrar un nuevo usuario en el sistema Amazon.
        /// La contraseña se hashea automáticamente antes de almacenarse.
        /// 
        /// Ejemplo de body:
        /// {
        ///     "login": "usuario123",
        ///     "password": "ContraseñaSegura123!",
        ///     "name": "Juan Pérez",
        ///     "role": "User"
        /// }
        /// 
        /// Roles disponibles: Administrator, Consumer, User
        /// Si no se especifica un rol, se asigna "User" por defecto.
        /// </remarks>
        /// <param name="securityDto">Datos del usuario a registrar</param>
        /// <returns>Usuario registrado con su identificador asignado</returns>
        /// <response code="200">Usuario registrado exitosamente</response>
        /// <response code="400">Error en la validación de datos o usuario ya existe</response>
        /// <response code="500">Error interno del servidor al registrar el usuario</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<SecurityDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]

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
