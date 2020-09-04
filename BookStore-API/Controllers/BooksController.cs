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
    /// Endpoint used to interact with the Books in the book store's database.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            ILoggerService logger,
            IMapper mapper
        )
        {
            _bookRepository = bookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all books
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks ()
        {
            try
            {
                _logger.LogInfo("Attempted get all books");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDto>>(books);

                _logger.LogInfo("Successfully got all books");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Gets a book by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Author record</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int id)
        {
            try
            {
                _logger.LogInfo($"Attempted get book with id:{id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"Book with id:{id} was not found.");
                    return NotFound();
                }

                var response = _mapper.Map<BookDto>(book);

                _logger.LogInfo($"Successfully got book with id:{id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Creates a book
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDto bookDto)
        {
            try
            {
                _logger.LogInfo("Book submission attempted.");
                if (bookDto == null)
                {
                    _logger.LogWarn("Empty request was submitted.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Book data was incomplete.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDto);
                var isCreated = await _bookRepository.Create(book);

                if (!isCreated)
                {
                    return InternalError("book creation failed");
                }

                _logger.LogInfo("Book created successfully.");
                return Created("Create", new { book });
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Updates a book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="book"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDto bookDto)
        {
            try
            {
                _logger.LogInfo($"Book with id:{id} update attempted..");
                if (id < 1 || bookDto == null || id != bookDto.Id)
                {
                    _logger.LogWarn("Book update failed with bad data.");
                    return BadRequest();
                }

                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                {
                    _logger.LogWarn($"Book with id:{id} was not found.");
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn("Book data was incomplete.");
                    return BadRequest(ModelState);
                }

                var book = _mapper.Map<Book>(bookDto);
                var isUpdated = await _bookRepository.Update(book);

                if (!isUpdated)
                {
                    return InternalError($"Failed to update book with id:{id}.");
                }

                _logger.LogInfo($"Book with id:{id} successfully updated.");
                return NoContent();
            }
            catch (Exception e)
            {
                return InternalError($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Deletes a book
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
                _logger.LogInfo($"Book with id:{id} delete attempted.");
                if (id < 1)
                {
                    _logger.LogWarn("Book delete failed with bad data");
                    return BadRequest();
                }

                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.LogWarn($"Book with id:{id} was not found.");
                    return NotFound();
                }

                var isDeleted = await _bookRepository.Delete(book);
                if (!isDeleted)
                {
                    return InternalError($"Failed to delete book with id:{id}.");
                }

                _logger.LogInfo($"Book with id:{id} successfully deleted.");
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
