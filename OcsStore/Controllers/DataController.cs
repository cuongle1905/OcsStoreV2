using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OcsStore.Models;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class DataController : Controller
    {
        private MyDbContext _context;

        public DataController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetUserManagementViews(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.UserManagementViews, loadOptions);
            return Ok(result);
        }

        private void SaveUser(User user)
        {
            if (user.Id == 0)
            {
                try
                {
                    user.Id = (short)(_context.Users.Max(i => i.Id) + 1);
                }
                catch
                {
                    user.Id = 1;
                }

                _context.Users.Add(user);
            }
            else
            {
                _context.Users.Update(user);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveUsers(User[] data)
        {
            foreach (User user in data)
            {
                SaveUser(user);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult GetExpenseTypes(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ExpenseTypes.OrderBy(i => i.Ordinal), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetExpenses(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.Expenses.OrderByDescending(i => i.Id).OrderByDescending(i => i.Date), loadOptions);
            return Ok(result);
        }

        private void SaveExpense(Expense expense)
        {
            expense.Date = Common.GetLocalDateWithoutTime(expense.Date); // Remove hour, minute...
            if (expense.Id == 0)
            {
                try
                {
                    expense.Id = (short)(_context.Expenses.Max(i => i.Id) + 1);
                }
                catch
                {
                    expense.Id = 1;
                }

                _context.Expenses.Add(expense);
            }
            else
            {
                _context.Expenses.Update(expense);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveExpenses(Expense[] data)
        {
            foreach (Expense expense in data)
            {
                SaveExpense(expense);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteExpense(int id)
        {
            var expense = _context.Expenses.FirstOrDefault(i => i.Id == id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
