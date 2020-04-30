using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using MyCompany.Data;
using MyCompany.Models;
using MyCompany.Test.Setup;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MyCompany.Test.Controllers {
    public class TeacherResourceIntTest {
        public TeacherResourceIntTest()
        {
            _factory = new NhipsterWebApplicationFactory<Startup>();
            _client = _factory.CreateClient();

            _applicationDatabaseContext = _factory.GetRequiredService<ApplicationDatabaseContext>();

            InitTest();
        }

        private const string DefaultName = "AAAAAAAAAA";
        private const string UpdatedName = "BBBBBBBBBB";

        private static readonly int? DefaultAge = 1;
        private static readonly int? UpdatedAge = 2;

        private readonly NhipsterWebApplicationFactory<Startup> _factory;
        private readonly HttpClient _client;

        private readonly ApplicationDatabaseContext _applicationDatabaseContext;

        private Teacher _teacher;

        private Teacher CreateEntity()
        {
            return new Teacher {
                Name = DefaultName,
                Age = DefaultAge
            };
        }

        private void InitTest()
        {
            _teacher = CreateEntity();
        }

        [Fact]
        public async Task CreateTeacher()
        {
            var databaseSizeBeforeCreate = _applicationDatabaseContext.Teachers.Count();

            // Create the Teacher
            var response = await _client.PostAsync("/api/teachers", TestUtil.ToJsonContent(_teacher));
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Validate the Teacher in the database
            var teacherList = _applicationDatabaseContext.Teachers.ToList();
            teacherList.Count().Should().Be(databaseSizeBeforeCreate + 1);
            var testTeacher = teacherList[teacherList.Count - 1];
            testTeacher.Name.Should().Be(DefaultName);
            testTeacher.Age.Should().Be(DefaultAge);
        }

        [Fact]
        public async Task CreateTeacherWithExistingId()
        {
            var databaseSizeBeforeCreate = _applicationDatabaseContext.Teachers.Count();
            databaseSizeBeforeCreate.Should().Be(0);
            // Create the Teacher with an existing ID
            _teacher.Id = 1L;

            // An entity with an existing ID cannot be created, so this API call must fail
            var response = await _client.PostAsync("/api/teachers", TestUtil.ToJsonContent(_teacher));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Validate the Teacher in the database
            var teacherList = _applicationDatabaseContext.Teachers.ToList();
            teacherList.Count().Should().Be(databaseSizeBeforeCreate);
        }

        [Fact]
        public async Task GetAllTeachers()
        {
            // Initialize the database
            _applicationDatabaseContext.Teachers.Add(_teacher);
            await _applicationDatabaseContext.SaveChangesAsync();

            // Get all the teacherList
            var response = await _client.GetAsync("/api/teachers?sort=id,desc");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectTokens("$.[*].id").Should().Contain(_teacher.Id);
            json.SelectTokens("$.[*].name").Should().Contain(DefaultName);
            json.SelectTokens("$.[*].age").Should().Contain(DefaultAge);
        }

        [Fact]
        public async Task GetTeacher()
        {
            // Initialize the database
            _applicationDatabaseContext.Teachers.Add(_teacher);
            await _applicationDatabaseContext.SaveChangesAsync();

            // Get the teacher
            var response = await _client.GetAsync($"/api/teachers/{_teacher.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectTokens("$.id").Should().Contain(_teacher.Id);
            json.SelectTokens("$.name").Should().Contain(DefaultName);
            json.SelectTokens("$.age").Should().Contain(DefaultAge);
        }

        [Fact]
        public async Task GetNonExistingTeacher()
        {
            var response = await _client.GetAsync("/api/teachers/" + long.MaxValue);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateTeacher()
        {
            // Initialize the database
            _applicationDatabaseContext.Teachers.Add(_teacher);
            await _applicationDatabaseContext.SaveChangesAsync();

            var databaseSizeBeforeUpdate = _applicationDatabaseContext.Teachers.Count();

            // Update the teacher
            var updatedTeacher =
                await _applicationDatabaseContext.Teachers.SingleOrDefaultAsync(it => it.Id == _teacher.Id);
            // Disconnect from session so that the updates on updatedTeacher are not directly saved in db
//TODO detach
            updatedTeacher.Name = UpdatedName;
            updatedTeacher.Age = UpdatedAge;

            var response = await _client.PutAsync("/api/teachers", TestUtil.ToJsonContent(updatedTeacher));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Validate the Teacher in the database
            var teacherList = _applicationDatabaseContext.Teachers.ToList();
            teacherList.Count().Should().Be(databaseSizeBeforeUpdate);
            var testTeacher = teacherList[teacherList.Count - 1];
            testTeacher.Name.Should().Be(UpdatedName);
            testTeacher.Age.Should().Be(UpdatedAge);
        }

        [Fact]
        public async Task UpdateNonExistingTeacher()
        {
            var databaseSizeBeforeUpdate = _applicationDatabaseContext.Teachers.Count();

            // If the entity doesn't have an ID, it will throw BadRequestAlertException
            var response = await _client.PutAsync("/api/teachers", TestUtil.ToJsonContent(_teacher));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Validate the Teacher in the database
            var teacherList = _applicationDatabaseContext.Teachers.ToList();
            teacherList.Count().Should().Be(databaseSizeBeforeUpdate);
        }

        [Fact]
        public async Task DeleteTeacher()
        {
            // Initialize the database
            _applicationDatabaseContext.Teachers.Add(_teacher);
            await _applicationDatabaseContext.SaveChangesAsync();

            var databaseSizeBeforeDelete = _applicationDatabaseContext.Teachers.Count();

            var response = await _client.DeleteAsync($"/api/teachers/{_teacher.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Validate the database is empty
            var teacherList = _applicationDatabaseContext.Teachers.ToList();
            teacherList.Count().Should().Be(databaseSizeBeforeDelete - 1);
        }

        [Fact]
        public void EqualsVerifier()
        {
            TestUtil.EqualsVerifier(typeof(Teacher));
            var teacher1 = new Teacher {
                Id = 1L
            };
            var teacher2 = new Teacher {
                Id = teacher1.Id
            };
            teacher1.Should().Be(teacher2);
            teacher2.Id = 2L;
            teacher1.Should().NotBe(teacher2);
            teacher1.Id = 0;
            teacher1.Should().NotBe(teacher2);
        }
    }
}
