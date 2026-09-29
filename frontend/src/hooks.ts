import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { createStudent, deleteStudent, listStudents } from './api';

const STUDENTS_KEY = ['students'];

export function useStudents() {
  return useQuery({ queryKey: STUDENTS_KEY, queryFn: listStudents });
}

export function useAddStudent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createStudent,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: STUDENTS_KEY }),
  });
}

export function useDeleteStudent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: deleteStudent,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: STUDENTS_KEY }),
  });
}
