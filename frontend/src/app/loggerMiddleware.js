const SENSITIVE = new Set(['password', 'Password']);

function redact(value) {
  if (Array.isArray(value)) {
    return value.map(redact);
  }
  if (value && typeof value === 'object') {
    return Object.fromEntries(
      Object.entries(value).map(([key, nested]) => [
        key,
        SENSITIVE.has(key) ? '[redacted]' : redact(nested),
      ]),
    );
  }
  return value;
}

export const actionLogger = () => (next) => (action) => {
  if (import.meta.env.DEV) {
    console.info('[pharmacy]', action.type, redact(action));
  }
  return next(action);
};
