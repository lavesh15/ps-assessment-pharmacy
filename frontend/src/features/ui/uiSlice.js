import { createSlice } from '@reduxjs/toolkit';

let toastId = 1;

const uiSlice = createSlice({
  name: 'ui',
  initialState: {
    toasts: [],
  },
  reducers: {
    showToast: {
      reducer(state, action) {
        state.toasts.push(action.payload);
      },
      prepare({ type = 'info', message }) {
        return {
          payload: {
            id: toastId++,
            type,
            message,
          },
        };
      },
    },
    dismissToast(state, action) {
      state.toasts = state.toasts.filter((toast) => toast.id !== action.payload);
    },
  },
});

export const { showToast, dismissToast } = uiSlice.actions;
export default uiSlice.reducer;
