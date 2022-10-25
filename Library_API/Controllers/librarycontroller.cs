using Library_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;

namespace Library_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class librarycontroller : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public librarycontroller(IConfiguration configuration)
        {
            _configuration = configuration;
        }  
        
        [HttpGet]
        public JsonResult Get()
        {
            string query =@"
             select authors.name as ""Author_name"", authors.DOB as ""Author_DOB"", 
                books.title as ""Book_Title"", books.DOP as ""Book_DOP""
             from authors
            join relation on relation.author = authors.name 
            join books on relation.book = books.title
            order by authors.name;
             ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("LibraryCon");
            NpgsqlDataReader myReader;
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();

                }
            }

            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult Post(authors_books authors_books)
        {
            string query = @"
            insert into books(title, DOP) 
                select @title, @DOP
                where not exists (
                    select 1 from books where title=@title and DOP=@DOP
                );
            insert into authors(name, DOB) 
                select @name, @DOB
                where not exists (
                    select 1 from authors where name=@name and DOB=@DOB
                );
            insert into relation(author,book)
                select @name, @title
                where not exists (
                    select 1 from relation where book=@title and author=@name
                );
            
        ";

            string sqlDataSource = _configuration.GetConnectionString("LibraryCon");
            NpgsqlDataReader myReader;

            DataTable table = new DataTable();
            using (NpgsqlConnection myCon = new NpgsqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (NpgsqlCommand myCommand = new NpgsqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@DOP", authors_books.DOP);
                    myCommand.Parameters.AddWithValue("@DOB", authors_books.DOB);
                    myCommand.Parameters.AddWithValue("@name", authors_books.name);
                    myCommand.Parameters.AddWithValue("@title", authors_books.title);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    myCon.Close();

                }
            }

            return new JsonResult("Added Sucessfully");
        }
    }
}
