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
    public class TeacherController : ControllerBase {
        private const string EntityName = "teacher";

        private readonly ApplicationDatabaseContext _applicationDatabaseContext;

        private readonly ILogger<TeacherController> _log;

        public TeacherController(ILogger<TeacherController> log,
            ApplicationDatabaseContext applicationDatabaseContext)
        {
            _log = log;
            _applicationDatabaseContext = applicationDatabaseContext;
        }

        [HttpPost("teachers")]
        [ValidateModel]
        public async Task<ActionResult<Teacher>> CreateTeacher([FromBody] Teacher teacher)
        {
            _log.LogDebug($"REST request to save Teacher : {teacher}");
            if (teacher.Id != 0)
                throw new BadRequestAlertException("A new teacher cannot already have an ID", EntityName, "idexists");
            _applicationDatabaseContext.AddGraph(teacher);
            await _applicationDatabaseContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacher)
                .WithHeaders(HeaderUtil.CreateEntityCreationAlert(EntityName, teacher.Id.ToString()));
        }

        [HttpPut("teachers")]
        [ValidateModel]
        public async Task<IActionResult> UpdateTeacher([FromBody] Teacher teacher)
        {
            _log.LogDebug($"REST request to update Teacher : {teacher}");
            if (teacher.Id == 0) throw new BadRequestAlertException("Invalid Id", EntityName, "idnull");
            //TODO catch //DbUpdateConcurrencyException into problem
            _applicationDatabaseContext.Update(teacher);
            await _applicationDatabaseContext.SaveChangesAsync();
            return Ok(teacher)
                .WithHeaders(HeaderUtil.CreateEntityUpdateAlert(EntityName, teacher.Id.ToString()));
        }

        [HttpGet("teachers")]
        public ActionResult<IEnumerable<Teacher>> GetAllTeachers(IPageable pageable)
        {
            _log.LogDebug("REST request to get a page of Teachers");
            var page = _applicationDatabaseContext.Teachers
                .UsePageable(pageable);
            var headers = PaginationUtil.GeneratePaginationHttpHeaders(page, HttpContext.Request);
            return Ok(page.Content).WithHeaders(headers);
        }

        [HttpGet("teachers/{id}")]
        public async Task<IActionResult> GetTeacher([FromRoute] long id)
        {
            _log.LogDebug($"REST request to get Teacher : {id}");
            var result = await _applicationDatabaseContext.Teachers
                .SingleOrDefaultAsync(teacher => teacher.Id == id);
            return ActionResultUtil.WrapOrNotFound(result);
        }

        [HttpDelete("teachers/{id}")]
        public async Task<IActionResult> DeleteTeacher([FromRoute] long id)
        {
            _log.LogDebug($"REST request to delete Teacher : {id}");
            _applicationDatabaseContext.Teachers.RemoveById(id);
            await _applicationDatabaseContext.SaveChangesAsync();
            return Ok().WithHeaders(HeaderUtil.CreateEntityDeletionAlert(EntityName, id.ToString()));
        }
    }
}
