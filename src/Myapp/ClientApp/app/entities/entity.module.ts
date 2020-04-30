import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

@NgModule({
  imports: [
    RouterModule.forChild([
      {
        path: 'teacher',
        loadChildren: () => import('./teacher/teacher.module').then(m => m.MyappTeacherModule)
      },
      {
        path: 'student',
        loadChildren: () => import('./student/student.module').then(m => m.MyappStudentModule)
      }
      /* jhipster-needle-add-entity-route - JHipster will add entity modules routes here */
    ])
  ]
})
export class MyappEntityModule {}
