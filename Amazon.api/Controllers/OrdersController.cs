using Amazon.api.Responses;
using Amazon.Core.CustomEntities;
using Amazon.Core.Entities;
using Amazon.Core.Enum;
using Amazon.Core.Interface;
using Amazon.Core.QueryFilters;
using Amazon.infrastructure.DTOs;
using Amazon.Infrastructure.DTOs;
using Amazon.Infrastructure.Repositories;
using Amazon.Infrastructure.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Net;

namespace Amazon.Api.Controllers
{
    /// <summary>
    /// Controlador para gestionar todas las operaciones relacionadas con órdenes y carritos de compra
    /// </summary>
    /// <remarks>
    /// Este controlador permite:
    /// - Gestionar órdenes completas
    /// - Manejar carritos de compra
    /// - Procesar pagos
    /// - Generar reportes de ventas
    /// </remarks>

    [Authorize(Roles = nameof(RoleType.Administrator))]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public OrdersController(
            IOrderService orderService,
            IMapper mapper,
            IValidationService validationService

            )
        {
            _orderService = orderService;
            _mapper = mapper;
            _validationService = validationService;
        }


        /// <summary>
        /// Recupera todas las órdenes registradas en el sistema
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders
        /// 
        /// Este endpoint devuelve una lista completa de todas las órdenes existentes en el sistema,
        /// incluyendo órdenes completadas, pendientes y carritos activos.
        /// </remarks>
        /// <returns>Lista paginada de órdenes con sus respectivos datos mapeados al DTO</returns>
        /// <response code="200">Retorna todas las órdenes exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderDto>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
       
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery]OrderQueryFilter orderQueryFilter)
        {
            try
            {
                var orders = await _orderService.GetAllOrder(orderQueryFilter);

                var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders.Pagination);

                var pagination = new Pagination
                {
                    TotalCount = orders.Pagination.TotalCount,
                    PageSize = orders.Pagination.PageSize,
                    CurrentPage = orders.Pagination.CurrentPage,
                    TotalPages = orders.Pagination.TotalPages,
                    HasNextPage = orders.Pagination.HasNextPage,
                    HasPreviousPage = orders.Pagination.HasPreviousPage
                };
                var response = new ApiResponse<IEnumerable<OrderDto>>(ordersDto)
                {
                    Pagination = pagination,
                    Messages = orders.Messages
                };

                return StatusCode((int)orders.StatusCode, response);
            }
            catch (Exception err)
            {
                var responsePost = new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Error", Description = err.Message } },
                };
                return StatusCode(500, responsePost);
            }
        }


        /// <summary>
        /// Obtiene una orden específica por su identificador único
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/5
        /// 
        /// Este endpoint recupera la información completa de una orden específica,
        /// incluyendo items, totales y estado actual.
        /// </remarks>
        /// <param name="id">Identificador único de la orden (mayor a 0)</param>
        /// <returns>Información detallada de la orden encontrada</returns>
        /// <response code="200">Orden encontrada exitosamente</response>
        /// <response code="400">Error en la validación del ID de la orden</response>
        /// <response code="404">No se encontró la orden con el ID especificado</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderId(int id)
        {
            try
            {
                var orders = await _orderService.GetByIdOrderAsync(id);
                var orderDto = _mapper.Map<OrderDto>(orders);
                var response = new ApiResponse<OrderDto>(orderDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }


        /// <summary>
        /// Crea una nueva orden simple en el sistema
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// POST /api/Orders
        /// 
        /// Ejemplo de body:
        /// {
        ///   "userId": 1,
        ///   "items": [
        ///     {
        ///       "productId": 5,
        ///       "quantity": 2,
        ///       "unitPrice": 29.99
        ///     }
        ///   ]
        /// }
        /// 
        /// Este endpoint permite registrar una nueva orden en el sistema validando los datos de entrada
        /// y creando la orden con estado inicial.
        /// </remarks>
        /// <param name="crearOrden">Objeto con la información completa de la orden a crear</param>
        /// <returns>Orden creada con su identificador asignado</returns>
        /// <response code="200">Orden creada exitosamente</response>
        /// <response code="400">Error de validación en los datos de entrada</response>
        /// <response code="500">Error interno del servidor al crear la orden</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpPost]
        public async Task<IActionResult> CreateSimpleOrder([FromBody] CrearOrdenRequest CrearOrden)
        {
            try {
                var validationResult = await _validationService.ValidateAsync(CrearOrden);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                var order = _mapper.Map<Order>(CrearOrden);
                await _orderService.CreatedOrder(order);

                var Orden = await _orderService.GetByIdOrderAsync(order.Id);
                var OrdenRequest = _mapper.Map<OrderDto>(Orden);
                var response = new ApiResponse<OrderDto>(OrdenRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error : {ex.Message}"));
            }
        }


        /// <summary>
        /// Elimina una orden del sistema por su identificador
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// DELETE /api/Orders/5
        /// 
        /// Este endpoint elimina permanentemente una orden del sistema.
        /// La eliminación es irreversible y solo puede realizarse en órdenes con estados específicos.
        /// </remarks>
        /// <param name="id">Identificador único de la orden a eliminar</param>
        /// <response code="204">Orden eliminada correctamente</response>
        /// <response code="404">No se encontró la orden a eliminar</response>
        /// <response code="500">Error interno del servidor al eliminar la orden</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDtoMapper(int id)
        {
            try
            {
                var order = await _orderService.GetByIdOrderAsync(id);
                await _orderService.DeleteAsync(order);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error : {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el carrito de compras activo de un usuario específico
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/user/1/cart
        /// 
        /// Este endpoint recupera el carrito de compras actual del usuario,
        /// incluyendo todos los items agregados y el total calculado.
        /// Si el usuario no tiene un carrito activo, se crea uno nuevo.
        /// </remarks>
        /// <param name="userId">Identificador único del usuario</param>
        /// <returns>Orden con estado de carrito y items agregados</returns>
        /// <response code="200">Carrito recuperado exitosamente</response>
        /// <response code="400">Error en la validación del ID del usuario</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpGet("user/{userId}/cart")]
        public async Task<IActionResult> GetUserCartDtoMapper(int userId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = userId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Errors = validationResult.Errors });
                }
                #endregion

                var cart = await _orderService.GetUserCartAsync(userId);
                var cartDto = _mapper.Map<OrderDto>(cart);
                var response = new ApiResponse<OrderDto>(cartDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }


        /// <summary>
        /// Agrega un producto al carrito de compras del usuario
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// POST /api/Orders/Product/5/Order/10/quantity/2/cart
        /// 
        /// Este endpoint permite agregar un producto específico al carrito de compras
        /// con la cantidad especificada. Si el producto ya existe en el carrito,
        /// se actualiza la cantidad.
        /// </remarks>
        /// <param name="productId">Identificador único del producto a agregar</param>
        /// <param name="orderId">Identificador único de la orden (carrito)</param>
        /// <param name="quantity">Cantidad del producto a agregar (mayor a 0)</param>
        /// <returns>Item del carrito agregado o actualizado</returns>
        /// <response code="200">Producto agregado al carrito exitosamente</response>
        /// <response code="400">Error de validación en los parámetros</response>
        /// <response code="500">Error interno del servidor al agregar el producto</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<OrderItemDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpPost("Product/{productId}/Order/{orderId}/quantity/{quantity}/cart")]
        public async Task<IActionResult> IntroduceItemCart(int productId, int orderId, int quantity)
        {
            try
            {
                var newItem=await _orderService.InsertProductIntoCart(productId,orderId,quantity);
                var newItemDto = _mapper.Map<OrderItemDto>(newItem);
                var response = new ApiResponse<OrderItemDto>(newItemDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);

            }
        }


        /// <summary>
        /// Elimina un producto específico del carrito de compras del usuario
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// DELETE /api/Orders/user/1/products/5
        /// 
        /// Este endpoint remueve completamente un producto del carrito de compras del usuario.
        /// La operación es irreversible y actualiza inmediatamente el total del carrito.
        /// </remarks>
        /// <param name="userId">Identificador único del usuario</param>
        /// <param name="productId">Identificador único del producto a eliminar</param>
        /// <response code="204">Producto eliminado del carrito exitosamente</response>
        /// <response code="404">Usuario o producto no encontrado</response>
        /// <response code="500">Error interno del servidor al eliminar el producto</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpDelete("user/{userId}/products/{productId}")]
        public async Task<IActionResult> EliminarItemCarrito(int userId, int productId)
        {

            await _orderService.DeleteItemAsync( userId, productId);
            return NoContent();


        }


        /// <summary>
        /// Procesa el pago del carrito de compras del usuario
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// POST /api/Orders/user/1/process-payment
        /// 
        /// Este endpoint inicia el proceso de pago para el carrito activo del usuario.
        /// Verifica el saldo disponible, procesa el pago y actualiza el estado de la orden.
        /// </remarks>
        /// <param name="userId">Identificador único del usuario</param>
        /// <returns>Resultado del proceso de pago con detalles de la transacción</returns>
        /// <response code="200">Pago procesado exitosamente</response>
        /// <response code="400">Saldo insuficiente o error en validación</response>
        /// <response code="500">Error interno del servidor al procesar el pago</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PaymentDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpPost("user/{userId}/process-payment")]
        public async Task<IActionResult> ProcessPaymentAsync(int userId)
        {
            try
            {
                var payment = await _orderService.ProcessPaymentAsync(userId);
                var paymentDto = _mapper.Map<PaymentDto>(payment);
                var response = new ApiResponse<PaymentDto>(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }

        }


        /// <summary>
        /// Obtiene el historial de órdenes completadas de un usuario específico
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/user/1/orders
        /// 
        /// Este endpoint recupera todas las órdenes pagadas y completadas del usuario,
        /// proporcionando un historial completo de compras realizadas.
        /// </remarks>
        /// <param name="userId">Identificador único del usuario</param>
        /// <returns>Lista de órdenes completadas del usuario</returns>
        /// <response code="200">Historial de órdenes recuperado exitosamente</response>
        /// <response code="400">Error en la validación del ID del usuario</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<OrderDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(ApiResponse<string>))]
        [HttpGet("user/{userId}/orders")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                #region Validaciones
                var validationRequest = new GetByIdRequest { Id = userId };
                var validationResult = await _validationService.ValidateAsync(validationRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(new ApiResponse<object>(new
                    {
                        Message = "Error de validación del ID de usuario",
                        Errors = validationResult.Errors
                    }));
                }
                #endregion

                var orders = await _orderService.GetAllOderUserAsync(userId);
                var ordersDto = _mapper.Map<IEnumerable<OrderDto>>(orders);

                var response = new ApiResponse<IEnumerable<OrderDto>>(ordersDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<string>($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el reporte mensual de ventas del sistema
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/dapper/1
        /// 
        /// Este endpoint genera un reporte detallado de ventas mensuales,
        /// incluyendo totales por mes, crecimiento y comparativas.
        /// Utiliza Dapper para optimizar el rendimiento de consultas complejas.
        /// </remarks>
        /// <returns>Reporte mensual de ventas con datos agregados</returns>
        /// <response code="200">Reporte generado exitosamente</response>
        /// <response code="500">Error interno del servidor al generar el reporte</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<ReporteMensualVentasResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("dapper/1")]
        public async Task<IActionResult> GetReporteMensualVentas()
        {
            var posts = await _orderService.GetReporteMensualVentas();


            var response = new ApiResponse<IEnumerable<ReporteMensualVentasResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene estadísticas generales del tablero de control
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/dapper/2
        /// 
        /// Este endpoint proporciona métricas clave para el dashboard administrativo,
        /// incluyendo total de ventas, usuarios registrados, órdenes activas, etc.
        /// </remarks>
        /// <returns>Estadísticas consolidados del sistema</returns>
        /// <response code="200">Estadísticas recuperadas exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<BoardStatsResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("dapper/2")]
        public async Task<IActionResult> GetBoardStats()
        {
            var posts = await _orderService.GetBoardStats();


            var response = new ApiResponse<IEnumerable<BoardStatsResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de productos más vendidos en el sistema
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/dapper/3
        /// 
        /// Este endpoint genera un ranking de los productos con mayores ventas,
        /// útil para análisis de inventario y estrategias comerciales.
        /// </remarks>
        /// <returns>Lista de productos ordenados por volumen de ventas</returns>
        /// <response code="200">Lista de productos más vendidos recuperada exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TopProductosVendidosResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("dapper/3")]
        public async Task<IActionResult> GetTopProductosVendidos()
        {
            var posts = await _orderService.GetTopProductosVendidos();


            var response = new ApiResponse<IEnumerable<TopProductosVendidosResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de productos con stock bajo
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/dapper/4
        /// 
        /// Este endpoint identifica productos que están por debajo del nivel mínimo de stock,
        /// permitiendo una gestión proactiva del inventario.
        /// </remarks>
        /// <returns>Lista de productos con stock crítico</returns>
        /// <response code="200">Lista de productos con bajo stock recuperada exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<LowStockProductResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("dapper/4")]
        public async Task<IActionResult> GetLowStockProductResponse()
        {
            var posts = await _orderService.GetLowStockProductResponse();


            var response = new ApiResponse<IEnumerable<LowStockProductResponse>> (posts);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene la lista de usuarios con mayor gasto total
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// GET /api/Orders/dapper/5
        /// 
        /// Este endpoint genera un ranking de usuarios según su gasto acumulado,
        /// útil para programas de fidelización y marketing dirigido.
        /// </remarks>
        /// <returns>Lista de usuarios ordenados por gasto total</returns>
        /// <response code="200">Lista de usuarios con mayor gasto recuperada exitosamente</response>
        /// <response code="500">Error interno del servidor al procesar la solicitud</response>
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TopUsersBySpendingResponse>>))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("dapper/5")]
        public async Task<IActionResult> GetTopUsersBySpending()
        {
            var posts = await _orderService.GetTopUsersBySpending();


            var response = new ApiResponse<IEnumerable<TopUsersBySpendingResponse>> (posts);

            return Ok(response);
        }

    }
}
