import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Navigate, useLocation } from 'react-router-dom';
import { login } from '../features/auth/authSlice';

export function LoginPage() {
  const dispatch = useDispatch();
  const { status, error } = useSelector((state) => state.auth);
  const location = useLocation();
  const [username, setUsername] = useState('admin');
  const [password, setPassword] = useState('Admin@123');
  const [submitting, setSubmitting] = useState(false);

  const from = location.state?.from?.pathname ?? '/';

  useEffect(() => {
    document.title = 'Sign in · ABC Pharmacy';
  }, []);

  if (status === 'authenticated') {
    return <Navigate to={from} replace />;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setSubmitting(true);
    try {
      await dispatch(login({ username, password })).unwrap();
    } catch {
      /* toast + slice handle the error */
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="login-layout">
      <form className="card login-card" onSubmit={handleSubmit}>
        <p className="eyebrow">ABC Pharmacy</p>
        <h1>Staff sign in</h1>
        <p className="muted">Demo account is pre-filled. Use it to open the medicine inventory.</p>
        <label>
          Username
          <input
            value={username}
            onChange={(event) => setUsername(event.target.value)}
            autoComplete="username"
            required
          />
        </label>
        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
            required
          />
        </label>
        {error?.detail && <p className="form-error">{error.detail}</p>}
        <button type="submit" disabled={submitting || status === 'unknown'}>
          {submitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
