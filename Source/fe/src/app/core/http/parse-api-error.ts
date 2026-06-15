import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extracts a user-facing message from an Angular HttpErrorResponse.
 * Backend uses RFC 9457 ProblemDetails: { title, detail, status }.
 */
export function parseApiError(err: HttpErrorResponse): string {
  if (err.status === 0) {
    return 'Cannot connect to server. Check your internet connection.';
  }

  if (err.status >= 500) {
    return 'Server error. Please try again later.';
  }

  // 4xx: backend detail is specific and safe to show
  const detail: string | undefined = err.error?.detail;
  if (detail) return detail;

  // Fallback for unexpected shapes
  return err.error?.title ?? err.message ?? 'Something went wrong.';
}
