using Microsoft.AspNetCore.Mvc;

namespace ftn.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class PostsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new[] { "Post 1", "Post 2" });
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        return Ok($"Post {id}");
    }

    [HttpPost]
    public IActionResult Create([FromBody] string title)
    {
        return CreatedAtAction(nameof(GetById), new { id = 1 }, title);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        return NoContent();
    }
}