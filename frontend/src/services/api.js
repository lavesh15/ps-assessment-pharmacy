import axios from 'axios';
import { API_BASE_URL, createCorrelationId, getCsrfToken } from './session';
import { toApiError } from './errors';

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
});

let store;

export function injectStore(reduxStore) {
  store = reduxStore;
}

api.interceptors.request.use((config) => {
  const csrf = getCsrfToken();
  if (csrf) {
    config.headers['X-CSRF-TOKEN'] = csrf;
  }
  config.headers['X-Correlation-ID'] = createCorrelationId();
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const apiError = toApiError(error);
    if (apiError.status === 401 && store && !error.config?.url?.includes('/auth/login')) {
      store.dispatch({ type: 'auth/sessionCleared' });
    }
    return Promise.reject(apiError);
  },
);

export default api;
