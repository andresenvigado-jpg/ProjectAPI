namespace API.Controllers
{
    using Business;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomerEntity = DataAccess.Data.Customer;
    using PostEntity = DataAccess.Data.Post;

    [Route("[controller]")]
    public class PostbulkController : Controller
    {
        private BaseService<CustomerEntity> CustomerService;
        private BaseService<PostEntity> PostService;
        public PostbulkController(BaseService<PostEntity> postService, BaseService<CustomerEntity> customerService)
        {
            PostService = postService;
            CustomerService = customerService;
        }

        [HttpPost("bulk")]
        public IActionResult CreateMultiplePosts([FromBody] List<PostEntity> posts)
        {
            if (posts == null || posts.Count == 0)
            {
                return BadRequest("La lista de posts no puede ser nula o vacía.");
            }

            var createdPosts = new List<PostEntity>();

            try
            {
                foreach (var post in posts)
                {
                    // Validar CustomerId
                    if (post.CustomerId == 0)
                    {
                        return BadRequest("Todos los posts deben tener un CustomerId válido.");
                    }

                    var customerExists = CustomerService.GetAll().Any(c => c.CustomerId == post.CustomerId);
                    if (!customerExists)
                    {
                        return NotFound($"No existe un cliente con el ID {post.CustomerId}.");
                    }

                    // Truncar Body si es necesario
                    if (!string.IsNullOrWhiteSpace(post.Body) && post.Body.Length > 20)
                    {
                        int maxLength = 97;
                        if (post.Body.Length > maxLength)
                        {
                            post.Body = post.Body.Substring(0, maxLength) + "...";
                        }
                    }

                    // Asignar Category según Type
                    switch (post.Type)
                    {
                        case 1:
                            post.Category = "Farándula";
                            break;
                        case 2:
                            post.Category = "Política";
                            break;
                        case 3:
                            post.Category = "Futbol";
                            break;
                            // Si no es 1, 2 o 3, se deja la categoría enviada
                    }

                    // Crear el Post
                    var createdPost = PostService.Create(post);
                    createdPosts.Add(createdPost);
                }

                return Ok(new
                {
                    message = $"{createdPosts.Count} posts creados exitosamente.",
                    data = createdPosts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }



    }
}
