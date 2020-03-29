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
    public class StudentResourceIntTest {
        public StudentResourceIntTest()
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

        private Student _student;

        private Student CreateEntity()
        {
            return new Student {
                Name = DefaultName,
                Age = DefaultAge
            };
        }

        private void InitTest()
        {
            _student = CreateEntity();
        }

        [Fact]
        public async Task CreateStudent()
        {
            var databaseSizeBeforeCreate = _applicationDatabaseContext.Students.Count();

            // Create the Student
            var response = await _client.PostAsync("/api/students", TestUtil.ToJsonContent(_student));
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Validate the Student in the database
            var studentList = _applicationDatabaseContext.Students.ToList();
            studentList.Count().Should().Be(databaseSizeBeforeCreate + 1);
            var testStudent = studentList[studentList.Count - 1];
            testStudent.Name.Should().Be(DefaultName);
            testStudent.Age.Should().Be(DefaultAge);
        }

        [Fact]
        public async Task CreateStudentWithExistingId()
        {
            var databaseSizeBeforeCreate = _applicationDatabaseContext.Students.Count();
            databaseSizeBeforeCreate.Should().Be(0);
            // Create the Student with an existing ID
            _student.Id = 1L;

            // An entity with an existing ID cannot be created, so this API call must fail
            var response = await _client.PostAsync("/api/students", TestUtil.ToJsonContent(_student));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Validate the Student in the database
            var studentList = _applicationDatabaseContext.Students.ToList();
            studentList.Count().Should().Be(databaseSizeBeforeCreate);
        }

        [Fact]
        public async Task GetAllStudents()
        {
            // Initialize the database
            _applicationDatabaseContext.Students.Add(_student);
            await _applicationDatabaseContext.SaveChangesAsync();

            // Get all the studentList
            var response = await _client.GetAsync("/api/students?sort=id,desc");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectTokens("$.[*].id").Should().Contain(_student.Id);
            json.SelectTokens("$.[*].name").Should().Contain(DefaultName);
            json.SelectTokens("$.[*].age").Should().Contain(DefaultAge);
        }

        [Fact]
        public async Task GetStudent()
        {
            // Initialize the database
            _applicationDatabaseContext.Students.Add(_student);
            await _applicationDatabaseContext.SaveChangesAsync();

            // Get the student
            var response = await _client.GetAsync($"/api/students/{_student.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = JToken.Parse(await response.Content.ReadAsStringAsync());
            json.SelectTokens("$.id").Should().Contain(_student.Id);
            json.SelectTokens("$.name").Should().Contain(DefaultName);
            json.SelectTokens("$.age").Should().Contain(DefaultAge);
        }

        [Fact]
        public async Task GetNonExistingStudent()
        {
            var response = await _client.GetAsync("/api/students/" + long.MaxValue);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateStudent()
        {
            // Initialize the database
            _applicationDatabaseContext.Students.Add(_student);
            await _applicationDatabaseContext.SaveChangesAsync();

            var databaseSizeBeforeUpdate = _applicationDatabaseContext.Students.Count();

            // Update the student
            var updatedStudent =
                await _applicationDatabaseContext.Students.SingleOrDefaultAsync(it => it.Id == _student.Id);
            // Disconnect from session so that the updates on updatedStudent are not directly saved in db
//TODO detach
            updatedStudent.Name = UpdatedName;
            updatedStudent.Age = UpdatedAge;

            var response = await _client.PutAsync("/api/students", TestUtil.ToJsonContent(updatedStudent));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Validate the Student in the database
            var studentList = _applicationDatabaseContext.Students.ToList();
            studentList.Count().Should().Be(databaseSizeBeforeUpdate);
            var testStudent = studentList[studentList.Count - 1];
            testStudent.Name.Should().Be(UpdatedName);
            testStudent.Age.Should().Be(UpdatedAge);
        }

        [Fact]
        public async Task UpdateNonExistingStudent()
        {
            var databaseSizeBeforeUpdate = _applicationDatabaseContext.Students.Count();

            // If the entity doesn't have an ID, it will throw BadRequestAlertException
            var response = await _client.PutAsync("/api/students", TestUtil.ToJsonContent(_student));
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Validate the Student in the database
            var studentList = _applicationDatabaseContext.Students.ToList();
            studentList.Count().Should().Be(databaseSizeBeforeUpdate);
        }

        [Fact]
        public async Task DeleteStudent()
        {
            // Initialize the database
            _applicationDatabaseContext.Students.Add(_student);
            await _applicationDatabaseContext.SaveChangesAsync();

            var databaseSizeBeforeDelete = _applicationDatabaseContext.Students.Count();

            var response = await _client.DeleteAsync($"/api/students/{_student.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Validate the database is empty
            var studentList = _applicationDatabaseContext.Students.ToList();
            studentList.Count().Should().Be(databaseSizeBeforeDelete - 1);
        }

        [Fact]
        public void EqualsVerifier()
        {
            TestUtil.EqualsVerifier(typeof(Student));
            var student1 = new Student {
                Id = 1L
            };
            var student2 = new Student {
                Id = student1.Id
            };
            student1.Should().Be(student2);
            student2.Id = 2L;
            student1.Should().NotBe(student2);
            student1.Id = 0;
            student1.Should().NotBe(student2);
        }
    }
}
