using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint used to interact with the Authors in the book store's database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(
            IAuthorRepository authorRepository,
            ILoggerService logger,
            IMapper mapper
        )
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.LogInfo("Attempted get all authors");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDto>>(authors);

                _logger.LogInfo("Successfully got all authors");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Get an author by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Author record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted get author with id:{id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Author with id:{id} was not found.");
                    return NotFound();
                }

                var response = _mapper.Map<AuthorDto>(author);

                _logger.LogInfo($"Successfully got author with id:{id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates an author
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDto authorDto)
        {
            try
            {
                _logger.LogInfo("Author submission attempted.");
                if (authorDto == null)
                {
                    _logger.LogWarn("Empty request was submitted.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete.");
                    return BadRequest(ModelState);
                }

                var author = _mapper.Map<Author>(authorDto);
                var isCreated = await _authorRepository.Create(author);

                if (!isCreated)
                {
                    return InternalError("Author creation failed");
                }

                _logger.LogInfo("Author created successfully.");
                return Created("Create", new { author });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Updates an author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="author"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDto authorDto)
        {
            try
            {
                _logger.LogInfo($"Author with id:{id} update attempted..");
                if (id < 1 || authorDto == null || id != authorDto.Id)
                {
                    _logger.LogWarn("Author update failed with bad data.");
                    return BadRequest();
                }

                var isExists = await _authorRepository.IsExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"Author with id:{id} was not found.");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Author data was incomplete.");
                    return BadRequest(ModelState);
                }
                
                var author = _mapper.Map<Author>(authorDto);
                var isUpdated = await _authorRepository.Update(author);

                if (!isUpdated)
                {
                    return InternalError($"Failed to update author with id:{id}.");
                }

                _logger.LogInfo($"Author with id:{id} successfully updated.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Deletes an author
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInfo($"Author with id:{id} delete attempted.");
                if (id < 1)
                {
                    _logger.LogWarn("Author delete failed with bad data");
                    return BadRequest();
                }

                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.LogWarn($"Author with id:{id} was not found.");
                    return NotFound();
                }

                var isDeleted = await _authorRepository.Delete(author);
                if (!isDeleted)
                {
                    return InternalError($"Failed to delete author with id:{id}.");
                }

                _logger.LogInfo($"Author with id:{id} successfully deleted.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        private ObjectResult InternalError(string message)
        {
            _logger.LogError(message);
            return StatusCode(500, "Something went wrong. Please contact the Administrator.");
        }
    }
}
