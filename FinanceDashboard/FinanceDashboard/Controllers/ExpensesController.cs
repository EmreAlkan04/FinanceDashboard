using FinanceDashboard.Client.Services;
using FinanceDashboard.Model;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace FinanceDashboard.Controllers
{
    /// <summary>
    /// HTTP API for expenses, reachable under /api/expenses.
    ///
    /// Keep this class thin: it only translates between HTTP (status codes, query/body)
    /// and IExpenseService. Validation rules and database access belong in the service,
    /// so a Razor page that adds an entry later goes through the same code.
    ///
    /// The methods are not marked "async" yet (nothing is awaited). Add the keyword
    /// as soon as you await the service.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpensesController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpGet("monthly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Dictionary<string, double>> GetMonthly([FromQuery] int? year)
        {
            year ??= DateTime.Now.Year;
            Dictionary<string, double> keyValuePairs = new Dictionary<string, double>();
            
            List<double> expenses;
            List<string> names = DateTimeFormatInfo.CurrentInfo.MonthNames.ToList();

            try
            {
                expenses = _expenseService.MonthlyExpense(year);
            }
            catch (BadHttpRequestException e)
            {
                throw new BadHttpRequestException("Request handling failed!", e.StatusCode);
            }

            for (int i = 0; i < names.Count - 1; i++)
            {
                keyValuePairs.Add(names[i], expenses[i]);
            }

            return Ok(keyValuePairs);

            //throw new NotImplementedException();
        }

        // ------------------------------------------------------------------
        // GET api/expenses/categories?year=2026
        // The costs per category of the selected year.
        // ------------------------------------------------------------------
        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IReadOnlyList<ExpenseDTO>> GetCategories([FromQuery] int? year)
        {

            year ??= DateTime.Now.Year;

            List<ExpenseDTO> expensesModels = new List<ExpenseDTO>();

            try
            {
                expensesModels = _expenseService.ExpenseByCategory(year)
                    .Select(s => new ExpenseDTO(s.Category, s.Expense)).ToList();

            }
            catch (BadHttpRequestException e)
            {
                throw new BadHttpRequestException($"Request handling failed!: {e.Message}", e.StatusCode);
            }

            if (expensesModels != null)
                return Ok(expensesModels);
            else
                return BadRequest();

        }

        // ------------------------------------------------------------------
        // POST api/expenses
        // Adds a new entry. Body: JSON matching ExpenseRequest.
        // ------------------------------------------------------------------
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<ActionResult<ExpensesModel>> Create([FromBody] ExpenseDTO request)
        {
            // TODO: [ApiController] already answers 400 automatically when the annotations on
            //       ExpenseRequest are violated; business rules (e.g. date in the future?) go in the service.
            // TODO: the service creates the entity, saves it and returns it (with the new Id).
            // TODO: answer 201 Created with the new entry. CreatedAtAction(...) also needs a
            //       GET api/expenses/{id} endpoint for the Location header; if you don't want one,
            //       use Created($"/api/expenses/{created.Id}", created) instead.
            throw new NotImplementedException();
        }

        // ------------------------------------------------------------------
        // PUT api/expenses/42
        // Replaces all editable fields of entry 42. Body: JSON matching ExpenseRequest.
        // (Use [HttpPatch] instead if callers should be able to send only the changed fields.)
        // ------------------------------------------------------------------
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IActionResult> Update(int id, [FromBody] ExpenseDTO request)
        {
            // TODO: the service looks up entry `id` and updates it.
            // TODO: unknown id -> return NotFound().
            // TODO: success -> return NoContent().
            throw new NotImplementedException();
        }
    }
}