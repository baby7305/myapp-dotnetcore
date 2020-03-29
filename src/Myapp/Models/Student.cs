using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCompany.Models {
    [Table("student")]
    public class Student {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }

        public int? Age { get; set; }

        public Teacher Teacher { get; set; }

        // jhipster-needle-entity-add-field - JHipster will add fields here, do not remove

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            var student = obj as Student;
            if (student?.Id == null || student?.Id == 0 || Id == 0) return false;
            return EqualityComparer<long>.Default.Equals(Id, student.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return "Student{" +
                    $"ID='{Id}'" +
                    $", Name='{Name}'" +
                    $", Age='{Age}'" +
                    "}";
        }
    }
}
