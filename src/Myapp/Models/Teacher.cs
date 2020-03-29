using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCompany.Models {
    [Table("teacher")]
    public class Teacher {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Name { get; set; }

        public int? Age { get; set; }

        public IList<Student> Students { get; set; } = new List<Student>();

        // jhipster-needle-entity-add-field - JHipster will add fields here, do not remove

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            var teacher = obj as Teacher;
            if (teacher?.Id == null || teacher?.Id == 0 || Id == 0) return false;
            return EqualityComparer<long>.Default.Equals(Id, teacher.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return "Teacher{" +
                    $"ID='{Id}'" +
                    $", Name='{Name}'" +
                    $", Age='{Age}'" +
                    "}";
        }
    }
}
