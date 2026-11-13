using System.ComponentModel.DataAnnotations;
using System.Globalization;
using FinanceDashboard.Client.DTO;
using FinanceDashboard.Client.Services;
using FinanceDashboard.Model;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDashboard.Controllers
{
    /// <summary>
    /// /api/expenses.
    /// ApiExceptionFilter, so the actions don't need try/catch.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private const int MinYear = 1;
        private const int MaxYear = 9999;

        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        /// <summary>
        /// GET api/expenses/monthly?year=2026
        /// Expense of each from the entered year.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("monthly")]
        [ProducesResponseType(typeof(Dictionary<string, double>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<Dictionary<string, double>> GetMonthly([FromQuery, Range(MinYear, MaxYear)] int? year)
        {
            var monthlyExpenses = _expenseService.MonthlyExpense(year);

            var monthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;

            var result = new Dictionary<string, double>();
            for (var month = 0; month < monthlyExpenses.Count; month++)
            {
                result[monthNames[month]] = monthlyExpenses[month];
            }

            return Ok(result);
        }

        /// <summary>
        /// GET api/expenses/categories?year=2026
        /// Expense of each category from the entered year.
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(IReadOnlyList<CategoryCostDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<IReadOnlyList<CategoryCostDTO>> GetCategories([FromQuery, Range(MinYear, MaxYear)] int? year)
        {
            var categories = _expenseService.ExpenseByCategory(year)
                .Select(c => new CategoryCostDTO(c.Category, c.Expense))
                .ToList();

            return Ok(categories);
        }


        /// <summary>
        /// GET api/expenses/42
        /// Return one single entry.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ExpensesModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ExpenseDTO> GetById(int id)
        {
            var expense = _expenseService.GetById(id);

            if (expense is null)
            {
                return NotFound();
            }

            ExpenseDTO dto = new ExpenseDTO
            {
                Category = expense.Category,
                Date = expense.Date,
                Expense = expense.Expense,
                Note = expense.Note
            };

            return Ok(dto);
        }

        /// <summary>
        /// POST api/expenses
        /// add new entry. Content under Body. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ExpensesModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<ExpensesModel> Create([FromBody] NewExpenseDTO request)
        {
            ExpenseDTO expenseDTO = new ExpenseDTO
            {
                Category = request.Category,
                Date = DateTime.Now,
                Expense = request.Expense,
                Note = request.Note,
                //Image = null
            }; 

            var created = _expenseService.AddExpense(expenseDTO);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// PUT api/expenses/{id}
        /// Update selected entry. Content from body.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(int id, [FromBody] ExpenseDTO request)
        {
            var updated = _expenseService.UpdateExpense(id, request);

            if (updated is null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
