const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5001';

let csrfToken = '';

export function setCsrfToken(token) {
  csrfToken = token ?? '';
}

export function getCsrfToken() {
  return csrfToken;
}

export function createIdempotencyKey() {
  return crypto.randomUUID();
}

export function createCorrelationId() {
  return crypto.randomUUID();
}

export { API_BASE_URL };
