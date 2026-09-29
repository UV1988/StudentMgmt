export interface Student {
  id: string;
  name: string;
  dateOfBirth: string; // ISO date, e.g. "2000-01-15"
  phone: string;
}

export interface NewStudent {
  name: string;
  dateOfBirth: string;
  phone: string;
}
