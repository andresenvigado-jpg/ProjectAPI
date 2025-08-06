namespace API.Controllers.Post
{
    using Business;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using PostEntity = DataAccess.Data.Post;
    using CustomerEntity = DataAccess.Data.Customer;
    using System;

    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private BaseService<CustomerEntity> CustomerService;
        private BaseService<PostEntity> PostService;
        public PostController(BaseService<PostEntity> postService, BaseService<CustomerEntity> customerService)
        {
            PostService = postService;
            CustomerService = customerService;
        }

        [HttpGet()]
        public IQueryable<PostEntity> GetAll()
        {
            return PostService.GetAll();
        }

        [HttpPost()]
        public PostEntity Create([FromBodyAttribute]  PostEntity entity)
        {
            try
            {
                if (entity == null)
                {
                    Response.StatusCode = 400; // Bad Request
                    Response.Headers.Add("X-Error-Message", "El cuerpo de la solicitud no puede ser nulo.");
                    return null;
                }

                if (entity.CustomerId == 0) // o null si es nullable
                {
                    Response.StatusCode = 400;
                    Response.Headers.Add("X-Error-Message", "El ID del cliente es obligatorio.");
                    return null;
                }

                // Validar si el Customer existe
                var existingCustomer = CustomerService.GetAll().FirstOrDefault(c => c.CustomerId == entity.CustomerId);
                if (existingCustomer == null)
                {
                    Response.StatusCode = 404; // Not Found
                    Response.Headers.Add("X-Error-Message", $"No existe un cliente con el ID {entity.CustomerId}.");
                    return null;
                }

                // Validar Body y truncar si excede 20 caracteres
                if (!string.IsNullOrWhiteSpace(entity.Body) && entity.Body.Length > 20)
                {
                    int maxLength = 97;
                    if (entity.Body.Length > maxLength)
                    {
                        entity.Body = entity.Body.Substring(0, maxLength) + "...";
                    }
                }

                // Asignar Category según Type
                switch (entity.Type)
                {
                    case 1:
                        entity.Category = "Farándula";
                        break;
                    case 2:
                        entity.Category = "Política";
                        break;
                    case 3:
                        entity.Category = "Futbol";
                        break;
                        // Si no es 1, 2 o 3, se mantiene lo que envió el usuario
                }
                return PostService.Create(entity);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                Response.Headers.Add("X-Error-Message", $"Error interno: {ex.Message}");
                return null;
            }
        }

        [HttpPut()]
        public PostEntity Update([FromBodyAttribute] PostEntity entity)
        {
            return PostService.Create(entity);
        }

        [HttpDelete()]
        public PostEntity Delete([FromBodyAttribute] PostEntity entity)
        {
            return PostService.Create(entity);
        }
    }
}
