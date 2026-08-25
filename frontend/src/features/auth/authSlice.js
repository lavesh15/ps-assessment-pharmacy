import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import api from '../../services/api';
import { setCsrfToken } from '../../services/session';
import { formatApiError } from '../../services/errors';
import { showToast } from '../ui/uiSlice';

export const bootstrapSession = createAsyncThunk(
  'auth/bootstrap',
  async (_, { rejectWithValue }) => {
    try {
      const csrf = await api.get('/api/v1/auth/csrf');
      setCsrfToken(csrf.data.token);
      const me = await api.get('/api/v1/auth/me');
      return me.data;
    } catch (error) {
      if (error.status === 401) {
        return rejectWithValue({ anonymous: true });
      }
      return rejectWithValue(error);
    }
  },
);

export const login = createAsyncThunk(
  'auth/login',
  async (credentials, { dispatch, rejectWithValue }) => {
    try {
      await api.get('/api/v1/auth/csrf').then((response) => setCsrfToken(response.data.token));
      const response = await api.post('/api/v1/auth/login', credentials);
      setCsrfToken(response.data.csrfToken);
      dispatch(showToast({ type: 'success', message: `Welcome, ${response.data.username}` }));
      return response.data;
    } catch (error) {
      dispatch(showToast({ type: 'error', message: formatApiError(error) }));
      return rejectWithValue(error);
    }
  },
);

export const logout = createAsyncThunk('auth/logout', async (_, { dispatch }) => {
  try {
    await api.post('/api/v1/auth/logout');
  } finally {
    setCsrfToken('');
    dispatch(showToast({ type: 'info', message: 'Signed out' }));
  }
});

const authSlice = createSlice({
  name: 'auth',
  initialState: {
    status: 'unknown',
    user: null,
    error: null,
  },
  reducers: {
    sessionCleared(state) {
      state.status = 'anonymous';
      state.user = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(bootstrapSession.pending, (state) => {
        state.status = 'unknown';
      })
      .addCase(bootstrapSession.fulfilled, (state, action) => {
        state.status = 'authenticated';
        state.user = { username: action.payload.username };
        state.error = null;
      })
      .addCase(bootstrapSession.rejected, (state, action) => {
        state.status = 'anonymous';
        state.user = null;
        state.error = action.payload?.anonymous ? null : action.payload;
      })
      .addCase(login.pending, (state) => {
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.status = 'authenticated';
        state.user = { username: action.payload.username };
        state.error = null;
      })
      .addCase(login.rejected, (state, action) => {
        state.status = 'anonymous';
        state.user = null;
        state.error = action.payload;
      })
      .addCase(logout.fulfilled, (state) => {
        state.status = 'anonymous';
        state.user = null;
      })
      .addCase(logout.rejected, (state) => {
        state.status = 'anonymous';
        state.user = null;
      });
  },
});

export const { sessionCleared } = authSlice.actions;
export default authSlice.reducer;
