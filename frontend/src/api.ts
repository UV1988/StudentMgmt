import type { NewStudent, Student } from './types';

const BASE_URL = '/api/students';

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly fieldErrors: Record<string, string[]> = {},
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function toApiError(response: Response): Promise<ApiError> {
  try {
    const body = await response.json();
    return new ApiError(body?.title ?? 'Request failed', response.status, body?.errors ?? {});
  } catch {
    return new ApiError('Request failed', response.status);
  }
}

export async function listStudents(): Promise<Student[]> {
  const response = await fetch(BASE_URL);
  if (!response.ok) throw await toApiError(response);
  return response.json();
}

export async function createStudent(student: NewStudent): Promise<Student> {
  const response = await fetch(BASE_URL, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(student),
  });
  if (!response.ok) throw await toApiError(response);
  return response.json();
}

export async function deleteStudent(id: string): Promise<void> {
  const response = await fetch(`${BASE_URL}/${id}`, { method: 'DELETE' });
  if (!response.ok) throw await toApiError(response);
}
