import { IStudent } from 'app/shared/model/student.model';

export interface ITeacher {
  id?: number;
  name?: string;
  age?: number;
  students?: IStudent[];
}

export class Teacher implements ITeacher {
  constructor(public id?: number, public name?: string, public age?: number, public students?: IStudent[]) {}
}
