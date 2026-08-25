export function toApiError(error) {
  const response = error.response;
  const data = response?.data;

  if (!response) {
    return {
      status: 0,
      title: 'Network error',
      detail: 'Unable to reach the pharmacy API. Confirm it is running on port 5001.',
      errors: undefined,
      errorCode: 'network_error',
      correlationId: undefined,
    };
  }

  const fieldErrors = data?.errors;
  const firstFieldError = fieldErrors
    ? Object.values(fieldErrors).flat()[0]
    : undefined;

  return {
    status: response.status,
    title: data?.title ?? 'Request failed',
    detail: firstFieldError ?? data?.detail ?? response.statusText,
    errors: fieldErrors,
    errorCode: data?.errorCode,
    correlationId: data?.correlationId ?? response.headers?.['x-correlation-id'],
  };
}

export function formatApiError(apiError) {
  if (apiError.status === 429) {
    return 'Too many requests. Please wait a moment and try again.';
  }
  if (apiError.status === 409) {
    return apiError.detail ?? 'This record changed. The list was refreshed.';
  }
  return apiError.detail ?? apiError.title ?? 'Something went wrong.';
}
