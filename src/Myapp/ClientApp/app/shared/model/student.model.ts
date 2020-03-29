import { ITeacher } from 'app/shared/model/teacher.model';

export interface IStudent {
  id?: number;
  name?: string;
  age?: number;
  teacher?: ITeacher;
}

export class Student implements IStudent {
  constructor(public id?: number, public name?: string, public age?: number, public teacher?: ITeacher) {}
}
