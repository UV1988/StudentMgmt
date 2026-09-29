import type { NewStudent } from './types';

export type FieldName = 'name' | 'dateOfBirth' | 'phone';
export type FieldErrors = Partial<Record<FieldName, string>>;

// Same rules as the API's CreateStudentRequestValidator. The server is still the source of truth;
// this only gives instant feedback before a request is sent.
const PHONE_PATTERN = /^\+?[0-9][0-9 \-()]{6,19}$/;
const MAX_AGE_YEARS = 120;

function toIsoDate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

export function validateStudent(input: NewStudent, today: Date = new Date()): FieldErrors {
  const errors: FieldErrors = {};

  const name = input.name.trim();
  if (!name) errors.name = "Enter the student's name.";
  else if (name.length > 100) errors.name = 'Name must be 100 characters or fewer.';

  if (!input.dateOfBirth) {
    errors.dateOfBirth = 'Enter a date of birth.';
  } else if (input.dateOfBirth > toIsoDate(today)) {
    errors.dateOfBirth = 'Date of birth cannot be in the future.';
  } else {
    const oldest = new Date(today.getFullYear() - MAX_AGE_YEARS, today.getMonth(), today.getDate());
    if (input.dateOfBirth < toIsoDate(oldest)) errors.dateOfBirth = 'Enter a valid date of birth.';
  }

  const phone = input.phone.trim();
  if (!phone) errors.phone = 'Enter a phone number.';
  else if (!PHONE_PATTERN.test(phone)) errors.phone = 'Enter a valid phone number, for example +91 98765 43210.';

  return errors;
}

/** Converts the API's validation problem ("Name": [...]) into the form's field keys. */
export function mapServerErrors(serverErrors: Record<string, string[]>): FieldErrors {
  const result: FieldErrors = {};
  for (const [key, messages] of Object.entries(serverErrors)) {
    const field = key.charAt(0).toLowerCase() + key.slice(1);
    if (field === 'name' || field === 'dateOfBirth' || field === 'phone') {
      result[field] = messages[0];
    }
  }
  return result;
}
