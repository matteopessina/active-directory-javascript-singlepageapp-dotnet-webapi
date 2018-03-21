using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using TodoSPA.DAL;
using TodoSPA.Models;
using TodoSPA.Filters;

namespace TodoSPA.Controllers
{
    [AppRolesAuthorization(AppRoles="TaskReader")]
    public class TodoListController : ApiController
    {
        private TodoListServiceContext db = new TodoListServiceContext();

        // GET: api/TodoList
        public IEnumerable<Todo> Get()
        {
            return db.Todoes.ToList();
        }

        // GET: api/TodoList/5
        public Todo Get(int id)
        {
            Todo todo = db.Todoes.First(a => a.ID == id);
            return todo;
        }

        // POST: api/TodoList
        [AppRolesAuthorization(AppRoles = "TaskCreator")]
        public void Post(Todo todo)
        {
            db.Todoes.Add(todo);
            db.SaveChanges();
        }

        // PUT: api/TodoList
        [AppRolesAuthorization(AppRoles = "TaskCreator")]
        public void Put(Todo todo)
        {
            Todo xtodo = db.Todoes.First(a => a.ID == todo.ID);
            if (todo != null)
            {
                xtodo.Description = todo.Description;
                db.SaveChanges();
            }
        }

        // DELETE: api/TodoList/5
        [AppRolesAuthorization(AppRoles = "TaskCreator")]
        public void Delete(int id)
        {
            Todo todo = db.Todoes.First(a => a.ID == id);
            if (todo != null)
            {
                db.Todoes.Remove(todo);
                db.SaveChanges();
            }
        }

    }
}
