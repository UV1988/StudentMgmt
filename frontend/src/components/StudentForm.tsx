import { useState, type ChangeEvent, type FormEvent } from 'react';
import { ApiError } from '../api';
import { useAddStudent } from '../hooks';
import type { NewStudent } from '../types';
import { mapServerErrors, validateStudent, type FieldErrors, type FieldName } from '../validation';

const EMPTY: NewStudent = { name: '', dateOfBirth: '', phone: '' };

export function StudentForm() {
  const addStudent = useAddStudent();
  const [values, setValues] = useState<NewStudent>(EMPTY);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [formError, setFormError] = useState<string | null>(null);

  const handleChange = (field: FieldName) => (event: ChangeEvent<HTMLInputElement>) =>
    setValues((current) => ({ ...current, [field]: event.target.value }));

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setFormError(null);

    const clientErrors = validateStudent(values);
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;

    addStudent.mutate(
      { name: values.name.trim(), dateOfBirth: values.dateOfBirth, phone: values.phone.trim() },
      {
        onSuccess: () => {
          setValues(EMPTY);
          setErrors({});
        },
        onError: (error) => {
          const serverErrors = error instanceof ApiError ? mapServerErrors(error.fieldErrors) : {};
          if (Object.keys(serverErrors).length > 0) setErrors(serverErrors);
          else setFormError("We couldn't save the student. Check your connection and try again.");
        },
      },
    );
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <Field id="name" label="Name" error={errors.name}>
        <input
          id="name"
          type="text"
          autoComplete="off"
          value={values.name}
          onChange={handleChange('name')}
          aria-invalid={Boolean(errors.name)}
          aria-describedby={errors.name ? 'name-error' : undefined}
        />
      </Field>

      <Field id="dateOfBirth" label="Date of birth" error={errors.dateOfBirth}>
        <input
          id="dateOfBirth"
          type="date"
          value={values.dateOfBirth}
          onChange={handleChange('dateOfBirth')}
          aria-invalid={Boolean(errors.dateOfBirth)}
          aria-describedby={errors.dateOfBirth ? 'dateOfBirth-error' : undefined}
        />
      </Field>

      <Field id="phone" label="Phone" error={errors.phone}>
        <input
          id="phone"
          type="tel"
          autoComplete="off"
          placeholder="+91 98765 43210"
          value={values.phone}
          onChange={handleChange('phone')}
          aria-invalid={Boolean(errors.phone)}
          aria-describedby={errors.phone ? 'phone-error' : undefined}
        />
      </Field>

      {formError && (
        <p className="form-error" role="alert">
          {formError}
        </p>
      )}

      <button type="submit" className="btn primary" disabled={addStudent.isPending}>
        {addStudent.isPending ? 'Adding…' : 'Add student'}
      </button>
    </form>
  );
}

interface FieldProps {
  id: string;
  label: string;
  error?: string;
  children: React.ReactNode;
}

function Field({ id, label, error, children }: FieldProps) {
  return (
    <div className="field">
      <label htmlFor={id}>{label}</label>
      {children}
      {error && (
        <p id={`${id}-error`} className="field-error" role="alert">
          {error}
        </p>
      )}
    </div>
  );
}
