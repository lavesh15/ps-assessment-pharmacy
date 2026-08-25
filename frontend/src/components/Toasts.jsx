import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { dismissToast } from '../features/ui/uiSlice';

export function Toasts() {
  const toasts = useSelector((state) => state.ui.toasts);
  const dispatch = useDispatch();

  useEffect(() => {
    const timers = toasts.map((toast) =>
      setTimeout(() => dispatch(dismissToast(toast.id)), 4500),
    );
    return () => timers.forEach(clearTimeout);
  }, [toasts, dispatch]);

  if (toasts.length === 0) {
    return null;
  }

  return (
    <div className="toast-stack" role="status">
      {toasts.map((toast) => (
        <button
          key={toast.id}
          type="button"
          className={`toast toast-${toast.type}`}
          onClick={() => dispatch(dismissToast(toast.id))}
        >
          {toast.message}
        </button>
      ))}
    </div>
  );
}
