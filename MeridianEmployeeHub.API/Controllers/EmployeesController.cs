using MeridianEmployeeHub.Services.Employees;
using MeridianEmployeeHub.Services.Employees.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController] // Îi spune framework-ului că acesta este un controller de API (activează validări automate)
    [Route("api/v1/[controller]")] // Rutele vor fi sub forma: /api/v1/employees
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        // Injectăm serviciul pe care l-am creat anterior
        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: api/v1/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees); // Returnează un status HTTP 200 (OK) cu lista de angajați
        }

        // GET: api/v1/employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound(); // Returnează status 404 (Not Found) dacă nu există
            }

            return Ok(employee);
        }

        // POST: api/v1/employees
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            var newEmployee = await _employeeService.CreateEmployeeAsync(request);

            // Returnează status 201 (Created) și rutează către metoda care poate aduce noul angajat
            return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployee.Id }, newEmployee);
        }
    }
}