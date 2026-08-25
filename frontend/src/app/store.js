import { configureStore } from '@reduxjs/toolkit';
import { actionLogger } from './loggerMiddleware';
import authReducer from '../features/auth/authSlice';
import medicinesReducer from '../features/medicines/medicinesSlice';
import uiReducer from '../features/ui/uiSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    medicines: medicinesReducer,
    ui: uiReducer,
  },
  middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(actionLogger),
});
