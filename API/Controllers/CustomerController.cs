namespace API.Controllers.Customer
{
    using Business;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;
    using CustomerEntity = DataAccess.Data.Customer;
    using PostEntity = DataAccess.Data.Post;

    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private BaseService<CustomerEntity> CustomerService;
        private BaseService<PostEntity> PostService;

        public CustomerController(BaseService<CustomerEntity> customerService, BaseService<PostEntity> postService)
        {
            CustomerService = customerService;
            PostService = postService;
        }
        [HttpGet()]
        public IQueryable<CustomerEntity> GetAll()
        {
            return CustomerService.GetAll();
        }
        //[HttpPost()]
        //public CustomerEntity Create([FromBodyAttribute] CustomerEntity entity)
        //{
        //    return CreateCustomer(entity);
        //}
        #region Validación al momento de crear un registro.  
        [HttpPost()]
        public CustomerEntity Create([FromBodyAttribute] CustomerEntity entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity), "El cuerpo de la solicitud no puede ser nulo.");

                if (string.IsNullOrWhiteSpace(entity.Name))
                    throw new ArgumentException("El nombre del cliente es obligatorio.", nameof(entity.Name));

                var existingCustomer = FindByName(entity.Name);
                if (existingCustomer != null)
                    throw new InvalidOperationException($"Ya existe un cliente con el nombre '{entity.Name}'.");

                return CustomerService.Create(entity);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                Response.Headers.Add("X-Error-Message", $"Error interno: {ex.Message}");
                return null;
            }
        }
        #endregion
        //private CustomerEntity CreateCustomer(CustomerEntity entity)
        //{
        //    throw new Exception("");
        //    return CustomerService.Create(entity);
        //}

        //Función que valida si el nombre de la persona existe en la base de datos. 
        [NonAction]
        public CustomerEntity FindByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(name));

            return GetAll().FirstOrDefault(c => c.Name == name);
        }
        //[HttpPut()]
        //public CustomerEntity Update(CustomerEntity entity)
        //{

        //    return CustomerService.Update(entity.CustomerId, entity, out bool changed);
        //}
        #region Validaciones adicionales, Captura de error 
        [HttpPut]
        public IActionResult Update([FromBody] CustomerEntity entity)
        {
            if (entity == null)
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");

            if (entity.CustomerId == null || entity.CustomerId.Equals(default))
                return BadRequest("El ID del cliente es obligatorio.");
            try
            {
                var updated = CustomerService.Update(entity.CustomerId, entity, out bool changed);

                if (updated == null)
                    return NotFound($"No se encontró un cliente con el ID {entity.CustomerId}.");

                return Ok(new
                {
                    message = changed ? "Cliente actualizado exitosamente." : "No hubo cambios en el cliente.",
                    data = updated
                });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Parámetro inválido: {ex.ParamName}");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        #endregion
        //[HttpDelete()]
        //public CustomerEntity Delete([FromBodyAttribute] CustomerEntity entity)
        //{
        //    return CustomerService.Delete(entity);
        //}

        [HttpDelete]
        public CustomerEntity Delete([FromBody] CustomerEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    Response.StatusCode = 400; // Bad Request
                    Response.Headers.Add("X-Error-Message", "El cuerpo de la solicitud no puede ser nulo.");
                    return null;
                }

                // Validar si el cliente existe
                var existingCustomer = CustomerService.GetAll().FirstOrDefault(c => c.CustomerId == entity.CustomerId);
                if (existingCustomer == null)
                {
                    Response.StatusCode = 404; // Not Found
                    Response.Headers.Add("X-Error-Message", $"No se encontró un cliente con el ID {entity.CustomerId}.");
                    return null;
                }

                // Buscar y eliminar posts relacionados
                var relatedPosts = PostService.GetAll().Where(p => p.CustomerId == entity.CustomerId).ToList();
                foreach (var post in relatedPosts)
                {
                    PostService.Delete(post);
                }

                // Eliminar el cliente
                return CustomerService.Delete(existingCustomer);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                Response.Headers.Add("X-Error-Message", $"Error interno: {ex.Message}");
                return null;
            }
        }
    }
}
