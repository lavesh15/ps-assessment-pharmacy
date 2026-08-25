import { useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { logout } from '../features/auth/authSlice';
import {
  createMedicine,
  fetchMedicines,
  sellMedicine,
  setSearch,
} from '../features/medicines/medicinesSlice';
import { Modal } from '../components/Modal';
import { formatApiError } from '../services/errors';

function daysUntil(dateStr) {
  const [year, month, day] = dateStr.split('-').map(Number);
  const expiry = new Date(year, month - 1, day);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return Math.round((expiry.getTime() - today.getTime()) / 86_400_000);
}

function rowClass(medicine) {
  if (daysUntil(medicine.expiryDate) < 30) {
    return 'row-expiring';
  }
  if (medicine.quantity < 10) {
    return 'row-low-stock';
  }
  return '';
}

const emptyForm = {
  fullName: '',
  brand: '',
  expiryDate: '',
  quantity: 1,
  price: '0.00',
  notes: '',
};

export function MedicinesPage() {
  const dispatch = useDispatch();
  const user = useSelector((state) => state.auth.user);
  const { items, status, error, search } = useSelector((state) => state.medicines);
  const [addOpen, setAddOpen] = useState(false);
  const [sellTarget, setSellTarget] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [sellQty, setSellQty] = useState(1);
  const [formError, setFormError] = useState(null);

  useEffect(() => {
    document.title = 'Medicines · ABC Pharmacy';
  }, []);

  useEffect(() => {
    const handle = setTimeout(() => {
      dispatch(fetchMedicines(search));
    }, 300);
    return () => clearTimeout(handle);
  }, [search, dispatch]);

  const legend = useMemo(
    () => [
      { className: 'swatch expiring', label: 'Expires in less than 30 days' },
      { className: 'swatch low-stock', label: 'Quantity below 10' },
    ],
    [],
  );

  async function handleAdd(event) {
    event.preventDefault();
    setFormError(null);
    try {
      await dispatch(
        createMedicine({
          fullName: form.fullName,
          brand: form.brand,
          expiryDate: form.expiryDate,
          quantity: Number(form.quantity),
          price: Number(form.price),
          notes: form.notes,
        }),
      ).unwrap();
      setAddOpen(false);
      setForm(emptyForm);
    } catch (apiError) {
      setFormError(formatApiError(apiError));
    }
  }

  async function handleSell(event) {
    event.preventDefault();
    if (!sellTarget) {
      return;
    }
    setFormError(null);
    try {
      await dispatch(
        sellMedicine({
          id: sellTarget.id,
          quantity: Number(sellQty),
          version: sellTarget.version,
        }),
      ).unwrap();
      setSellTarget(null);
      setSellQty(1);
    } catch (apiError) {
      setFormError(formatApiError(apiError));
    }
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">ABC Pharmacy</p>
          <h1>Medicine inventory</h1>
        </div>
        <div className="topbar-actions">
          <span className="muted">Signed in as {user?.username}</span>
          <button type="button" className="secondary" onClick={() => dispatch(logout())}>
            Sign out
          </button>
        </div>
      </header>

      <section className="toolbar">
        <input
          className="search"
          placeholder="Search by medicine name"
          value={search}
          onChange={(event) => dispatch(setSearch(event.target.value))}
        />
        <button type="button" onClick={() => setAddOpen(true)}>
          Add medicine
        </button>
      </section>

      <div className="legend">
        {legend.map((item) => (
          <span key={item.label}>
            <i className={item.className} /> {item.label}
          </span>
        ))}
      </div>

      {status === 'failed' && <p className="form-error">{formatApiError(error)}</p>}

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Brand</th>
              <th>Expiry</th>
              <th>Qty</th>
              <th>Price</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && status !== 'loading' && (
              <tr>
                <td colSpan={6} className="empty">
                  No medicines match this search.
                </td>
              </tr>
            )}
            {items.map((medicine) => (
              <tr key={medicine.id} className={rowClass(medicine)}>
                <td>{medicine.fullName}</td>
                <td>{medicine.brand}</td>
                <td>{medicine.expiryDate}</td>
                <td>{medicine.quantity}</td>
                <td>{Number(medicine.price).toFixed(2)}</td>
                <td>
                  <button
                    type="button"
                    className="secondary"
                    onClick={() => {
                      setSellTarget(medicine);
                      setSellQty(1);
                      setFormError(null);
                    }}
                    disabled={medicine.quantity < 1}
                  >
                    Sell
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {addOpen && (
        <Modal
          title="Add medicine"
          onClose={() => {
            setAddOpen(false);
            setFormError(null);
          }}
        >
          <form className="form-grid" onSubmit={handleAdd}>
            <label>
              Full name
              <input
                value={form.fullName}
                onChange={(event) => setForm({ ...form, fullName: event.target.value })}
                required
                maxLength={200}
              />
            </label>
            <label>
              Brand
              <input
                value={form.brand}
                onChange={(event) => setForm({ ...form, brand: event.target.value })}
                required
                maxLength={120}
              />
            </label>
            <label>
              Expiry date
              <input
                type="date"
                value={form.expiryDate}
                onChange={(event) => setForm({ ...form, expiryDate: event.target.value })}
                required
              />
            </label>
            <label>
              Quantity
              <input
                type="number"
                min="0"
                value={form.quantity}
                onChange={(event) => setForm({ ...form, quantity: event.target.value })}
                required
              />
            </label>
            <label>
              Price
              <input
                type="number"
                min="0"
                step="0.01"
                value={form.price}
                onChange={(event) => setForm({ ...form, price: event.target.value })}
                required
              />
            </label>
            <label className="full">
              Notes
              <textarea
                rows={3}
                value={form.notes}
                onChange={(event) => setForm({ ...form, notes: event.target.value })}
                maxLength={2000}
              />
            </label>
            {formError && <p className="form-error full">{formError}</p>}
            <div className="modal-actions full">
              <button type="button" className="secondary" onClick={() => setAddOpen(false)}>
                Cancel
              </button>
              <button type="submit">Save</button>
            </div>
          </form>
        </Modal>
      )}

      {sellTarget && (
        <Modal
          title={`Sell ${sellTarget.fullName}`}
          onClose={() => {
            setSellTarget(null);
            setFormError(null);
          }}
        >
          <form className="form-grid" onSubmit={handleSell}>
            <p className="muted full">
              In stock: {sellTarget.quantity}. This writes an audit sale record and reduces quantity.
            </p>
            <label>
              Quantity to sell
              <input
                type="number"
                min="1"
                max={sellTarget.quantity}
                value={sellQty}
                onChange={(event) => setSellQty(event.target.value)}
                required
              />
            </label>
            {formError && <p className="form-error full">{formError}</p>}
            <div className="modal-actions full">
              <button type="button" className="secondary" onClick={() => setSellTarget(null)}>
                Cancel
              </button>
              <button type="submit">Confirm sale</button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  );
}
