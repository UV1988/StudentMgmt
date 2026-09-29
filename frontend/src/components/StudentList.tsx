import { useState } from 'react';
import { useDeleteStudent, useStudents } from '../hooks';
import type { Student } from '../types';

function formatDate(isoDate: string): string {
  return new Date(`${isoDate}T00:00:00`).toLocaleDateString(undefined, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function StudentList() {
  const { data: students, isPending, isError, refetch } = useStudents();
  const deleteStudent = useDeleteStudent();
  const [confirmingId, setConfirmingId] = useState<string | null>(null);

  if (isPending) return <p className="state">Loading students…</p>;

  if (isError) {
    return (
      <div className="state" role="alert">
        <p>We couldn't load the students. Check that the API is running, then try again.</p>
        <button type="button" className="btn" onClick={() => refetch()}>
          Try again
        </button>
      </div>
    );
  }

  if (students.length === 0) {
    return <p className="state">No students yet. Use the form to add the first one.</p>;
  }

  function confirmDelete(student: Student) {
    deleteStudent.mutate(student.id, { onSettled: () => setConfirmingId(null) });
  }

  return (
    <>
      {deleteStudent.isError && (
        <p className="form-error" role="alert">
          We couldn't delete that student. Try again.
        </p>
      )}
      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Date of birth</th>
              <th scope="col">Phone</th>
              <th scope="col">
                <span className="visually-hidden">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {students.map((student) => (
              <tr key={student.id}>
                <td>{student.name}</td>
                <td>{formatDate(student.dateOfBirth)}</td>
                <td>{student.phone}</td>
                <td className="actions">
                  {confirmingId === student.id ? (
                    <>
                      <button
                        type="button"
                        className="btn danger"
                        disabled={deleteStudent.isPending}
                        onClick={() => confirmDelete(student)}
                      >
                        {deleteStudent.isPending ? 'Deleting…' : `Delete ${student.name}`}
                      </button>
                      <button type="button" className="btn" onClick={() => setConfirmingId(null)}>
                        Keep
                      </button>
                    </>
                  ) : (
                    <button
                      type="button"
                      className="btn"
                      aria-label={`Delete ${student.name}`}
                      onClick={() => setConfirmingId(student.id)}
                    >
                      Delete
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
