import { describe, expect, it } from 'vitest';
import { mapServerErrors, validateStudent } from './validation';

const today = new Date(2026, 8, 28); // 28 Sep 2026
const valid = { name: 'Asha Rao', dateOfBirth: '2000-01-15', phone: '+91 98765 43210' };

describe('validateStudent', () => {
  it('accepts a valid student', () => {
    expect(validateStudent(valid, today)).toEqual({});
  });

  it('requires a name', () => {
    expect(validateStudent({ ...valid, name: '   ' }, today).name).toBeDefined();
  });

  it('rejects a date of birth in the future', () => {
    expect(validateStudent({ ...valid, dateOfBirth: '2026-09-29' }, today).dateOfBirth).toBeDefined();
  });

  it('allows a date of birth of today', () => {
    expect(validateStudent({ ...valid, dateOfBirth: '2026-09-28' }, today).dateOfBirth).toBeUndefined();
  });

  it('rejects a date of birth more than 120 years ago', () => {
    expect(validateStudent({ ...valid, dateOfBirth: '1900-01-01' }, today).dateOfBirth).toBeDefined();
  });

  it.each(['abc', '12345', '+', '123456789012345678901'])('rejects phone "%s"', (phone) => {
    expect(validateStudent({ ...valid, phone }, today).phone).toBeDefined();
  });

  it.each(['+91 98765 43210', '9876543210', '020-2555-1234'])('accepts phone "%s"', (phone) => {
    expect(validateStudent({ ...valid, phone }, today).phone).toBeUndefined();
  });
});

describe('mapServerErrors', () => {
  it('maps PascalCase API keys to form field names', () => {
    expect(mapServerErrors({ Name: ['Bad name'], DateOfBirth: ['Bad date'], Other: ['x'] })).toEqual({
      name: 'Bad name',
      dateOfBirth: 'Bad date',
    });
  });
});
