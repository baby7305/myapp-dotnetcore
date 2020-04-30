using System.Collections.Generic;
using System.Threading.Tasks;
using JHipsterNet.Pagination;
using JHipsterNet.Pagination.Extensions;
using MyCompany.Data;
using MyCompany.Data.Extensions;
using MyCompany.Models;
using MyCompany.Web.Extensions;
using MyCompany.Web.Filters;
using MyCompany.Web.Rest.Problems;
using MyCompany.Web.Rest.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyCompany.Controllers {
    [Authorize]
    [Route("api")]
    [ApiController]
    public class StudentController : ControllerBase {
        private const string EntityName = "student";

        private readonly ApplicationDatabaseContext _applicationDatabaseContext;

        private readonly ILogger<StudentController> _log;

        public StudentController(ILogger<StudentController> log,
            ApplicationDatabaseContext applicationDatabaseContext)
        {
            _log = log;
            _applicationDatabaseContext = applicationDatabaseContext;
        }

        [HttpPost("students")]
        [ValidateModel]
        public async Task<ActionResult<Student>> CreateStudent([FromBody] Student student)
        {
            _log.LogDebug($"REST request to save Student : {student}");
            if (student.Id != 0)
                throw new BadRequestAlertException("A new student cannot already have an ID", EntityName, "idexists");
            _applicationDatabaseContext.AddGraph(student);
            await _applicationDatabaseContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student)
                .WithHeaders(HeaderUtil.CreateEntityCreationAlert(EntityName, student.Id.ToString()));
        }

        [HttpPut("students")]
        [ValidateModel]
        public async Task<IActionResult> UpdateStudent([FromBody] Student student)
        {
            _log.LogDebug($"REST request to update Student : {student}");
            if (student.Id == 0) throw new BadRequestAlertException("Invalid Id", EntityName, "idnull");
            //TODO catch //DbUpdateConcurrencyException into problem
            _applicationDatabaseContext.Update(student);
            /* Force the reference navigation property to be in "modified" state.
            This allows to modify it with a null value (the field is nullable).
            This takes into consideration the case of removing the association between the two instances. */
            _applicationDatabaseContext.Entry(student).Reference(student0 => student0.Teacher).IsModified = true;
            await _applicationDatabaseContext.SaveChangesAsync();
            return Ok(student)
                .WithHeaders(HeaderUtil.CreateEntityUpdateAlert(EntityName, student.Id.ToString()));
        }

        [HttpGet("students")]
        public ActionResult<IEnumerable<Student>> GetAllStudents(IPageable pageable)
        {
            _log.LogDebug("REST request to get a page of Students");
            var page = _applicationDatabaseContext.Students
                .Include(student => student.Teacher)
                .UsePageable(pageable);
            var headers = PaginationUtil.GeneratePaginationHttpHeaders(page, HttpContext.Request);
            return Ok(page.Content).WithHeaders(headers);
        }

        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudent([FromRoute] long id)
        {
            _log.LogDebug($"REST request to get Student : {id}");
            var result = await _applicationDatabaseContext.Students
                .Include(student => student.Teacher)
                .SingleOrDefaultAsync(student => student.Id == id);
            return ActionResultUtil.WrapOrNotFound(result);
        }

        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent([FromRoute] long id)
        {
            _log.LogDebug($"REST request to delete Student : {id}");
            _applicationDatabaseContext.Students.RemoveById(id);
            await _applicationDatabaseContext.SaveChangesAsync();
            return Ok().WithHeaders(HeaderUtil.CreateEntityDeletionAlert(EntityName, id.ToString()));
        }
    }
}
