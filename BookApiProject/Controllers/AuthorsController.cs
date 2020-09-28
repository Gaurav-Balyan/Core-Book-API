using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Dtos;
using BookApiProject.Models;
using BookApiProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController: Controller
    {
        private ICountryRepository _countryRepository;
        private IAuthorRepository _authorRepository;
        private IBookRepository _bookRepository;

        public AuthorsController(ICountryRepository countryRepository, 
                                 IAuthorRepository authorRepository, 
                                 IBookRepository bookRepository
        )
        {
            _countryRepository = countryRepository;
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
        }

        //api/authors
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        public IActionResult GetAuthors() {
            var authors = _authorRepository.GetAuthors().ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorsDto = new List<AuthorDto>();
            foreach (var author in authors) {
                authorsDto.Add(new AuthorDto {
                    Id = author.Id,
                    FirstName= author.FirstName,
                    LastName= author.LastName
                });
            }

            return Ok(authorsDto);
        }

        //api/authors/authorId
        [HttpGet("{authorId}", Name ="GetAuthor")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(AuthorDto))]
        public IActionResult GetAuthor(int authorId) {
            if (!_authorRepository.AuthorExists(authorId)) {
                return NotFound();
            }

            var author = _authorRepository.GetAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorDto = new AuthorDto { 
            Id= author.Id,
            FirstName= author.FirstName,
            LastName= author.LastName
            };

            return Ok(authorDto);
        }

        //api/authors/authorId/books
        [HttpGet("{authorId}/books")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        public IActionResult GetBooksByAuthor(int authorId) {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var books = _authorRepository.GetBooksByAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booksDto = new List<BookDto>();
            foreach (var book in books) {
                booksDto.Add(new BookDto { 
                Id= book.Id,
                Title=book.Title,
                Isbn= book.Isbn,
                DatePublished= book.DatePublished
                });
            }

            return Ok(booksDto);
        }

        //api/authors/books/bookId
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        public IActionResult GetAuthorsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var authors = _authorRepository.GetAuthorsOfABook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorsDto = new List<AuthorDto>();
            foreach (var author in authors) {
                authorsDto.Add(new AuthorDto { 
                Id= author.Id,
                FirstName= author.FirstName,
                LastName= author.LastName
                });
            }

            return Ok(authorsDto);
        }

        //api/authors
        [HttpPost]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        [ProducesResponseType(201, Type = typeof(Author))]
        public IActionResult CreateAuthor([FromBody] Author authorToCreate)
        {
            if (authorToCreate == null)
            {
                return BadRequest(ModelState);
            }

            if (!_countryRepository.CountryExists(authorToCreate.Country.Id))
            {
                ModelState.AddModelError("", "Country dosn't exist!");
                return StatusCode(404, ModelState);
            }

            authorToCreate.Country = _countryRepository.GetCountry(authorToCreate.Country.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_authorRepository.CreateAuthor(authorToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong saving the author {authorToCreate.FirstName} {authorToCreate.LastName}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetAuthor", new { authorId = authorToCreate.Id }, authorToCreate);
        }

        //api/authors/authorId
        [HttpPut("{authorId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult UpdateAuthor(int authorId, [FromBody] Author updatedAuthorInfo)
        {
            if (updatedAuthorInfo == null)
            {
                return BadRequest(ModelState);
            }

            if (authorId != updatedAuthorInfo.Id)
            {
                return BadRequest(ModelState);
            }

            if (!_authorRepository.AuthorExists(authorId))
            {
                ModelState.AddModelError("", "Author doesn't exist!");
            }

            if (!_countryRepository.CountryExists(updatedAuthorInfo.Country.Id))
            {
                ModelState.AddModelError("", "Country doesn't exist!");
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(404, ModelState);
            }

            updatedAuthorInfo.Country = _countryRepository.GetCountry(updatedAuthorInfo.Country.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_authorRepository.UpdateAuthor(updatedAuthorInfo))
            {
                ModelState.AddModelError("", $"Something went wrong updating the author {updatedAuthorInfo.FirstName} {updatedAuthorInfo.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/authors/authorId
        [HttpDelete("{authorId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        [ProducesResponseType(204)]
        public IActionResult DeleteAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var authorToDelete = _authorRepository.GetAuthor(authorId);

            if (_authorRepository.GetBooksByAuthor(authorId).Count() > 0)
            {
                ModelState.AddModelError("", $"Author {authorToDelete.FirstName} {authorToDelete.LastName} cannot be deleted because it is associated with atleast one book");
                return StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!_authorRepository.DeleteAuthor(authorToDelete))
            {
                ModelState.AddModelError("", $"Something went wrong deleting {authorToDelete.FirstName} {authorToDelete.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
