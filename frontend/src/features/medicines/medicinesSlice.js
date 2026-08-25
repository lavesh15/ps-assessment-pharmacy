import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import api from '../../services/api';
import { createIdempotencyKey } from '../../services/session';
import { formatApiError } from '../../services/errors';
import { showToast } from '../ui/uiSlice';

export const fetchMedicines = createAsyncThunk(
  'medicines/fetch',
  async (search, { rejectWithValue }) => {
    try {
      const response = await api.get('/api/v1/medicines', { params: { search: search || undefined } });
      return response.data;
    } catch (error) {
      return rejectWithValue(error);
    }
  },
);

export const createMedicine = createAsyncThunk(
  'medicines/create',
  async (payload, { dispatch, rejectWithValue }) => {
    try {
      const response = await api.post('/api/v1/medicines', payload, {
        headers: { 'Idempotency-Key': createIdempotencyKey() },
      });
      dispatch(showToast({ type: 'success', message: `${response.data.fullName} added` }));
      return response.data;
    } catch (error) {
      dispatch(showToast({ type: 'error', message: formatApiError(error) }));
      return rejectWithValue(error);
    }
  },
);

export const sellMedicine = createAsyncThunk(
  'medicines/sell',
  async ({ id, quantity, version }, { dispatch, getState, rejectWithValue }) => {
    try {
      const response = await api.post(
        `/api/v1/medicines/${id}/sell`,
        { quantity, version },
        { headers: { 'Idempotency-Key': createIdempotencyKey() } },
      );
      dispatch(showToast({ type: 'success', message: `Sold ${quantity} unit(s)` }));
      return response.data;
    } catch (error) {
      dispatch(showToast({ type: 'error', message: formatApiError(error) }));
      if (error.status === 409) {
        dispatch(fetchMedicines(getState().medicines.search));
      }
      return rejectWithValue(error);
    }
  },
);

const medicinesSlice = createSlice({
  name: 'medicines',
  initialState: {
    items: [],
    status: 'idle',
    error: null,
    search: '',
  },
  reducers: {
    setSearch(state, action) {
      state.search = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchMedicines.pending, (state) => {
        state.status = 'loading';
        state.error = null;
      })
      .addCase(fetchMedicines.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
      })
      .addCase(fetchMedicines.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload;
      })
      .addCase(createMedicine.fulfilled, (state, action) => {
        const listItem = {
          id: action.payload.id,
          fullName: action.payload.fullName,
          expiryDate: action.payload.expiryDate,
          quantity: action.payload.quantity,
          price: action.payload.price,
          brand: action.payload.brand,
          version: action.payload.version,
        };
        state.items.push(listItem);
        state.items.sort((a, b) => a.fullName.localeCompare(b.fullName));
      })
      .addCase(sellMedicine.fulfilled, (state, action) => {
        const item = state.items.find((medicine) => medicine.id === action.payload.medicineId);
        if (item) {
          item.quantity = action.payload.remainingQuantity;
          item.version = action.payload.version;
        }
      });
  },
});

export const { setSearch } = medicinesSlice.actions;
export default medicinesSlice.reducer;
