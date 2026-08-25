import { useSelector } from 'react-redux';
import { Navigate, useLocation } from 'react-router-dom';

export function ProtectedRoute({ children }) {
  const status = useSelector((state) => state.auth.status);
  const location = useLocation();

  if (status === 'unknown') {
    return <div className="page-loading">Checking session…</div>;
  }

  if (status !== 'authenticated') {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return children;
}
