﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApiProject.Models;

namespace BookApiProject.Services
{
    public interface IBookRepository
    {
        ICollection<Book> GetBooks();
        Book GetBook(int bookId);
        Book GetBook(string bookIsbn);
        decimal GetBookRating(int bookId);
        bool BookExists(int bookId);
        bool BookExists(string bookIsbn);
        bool IsDuplicateISBN(int bookId, string bookIsbn);
        bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book);
        bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book);   
        bool DeleteBook(Book book);
        bool Save();

    }
}