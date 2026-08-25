import { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { Navigate, Route, Routes } from 'react-router-dom';
import { bootstrapSession } from './features/auth/authSlice';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Toasts } from './components/Toasts';
import { LoginPage } from './pages/LoginPage';
import { MedicinesPage } from './pages/MedicinesPage';

export default function App() {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(bootstrapSession());
  }, [dispatch]);

  return (
    <>
      <Toasts />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <MedicinesPage />
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  );
}
