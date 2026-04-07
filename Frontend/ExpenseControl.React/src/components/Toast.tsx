import { useState, useEffect, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { ToastContext } from '../hooks/useToast';

type ToastVariant = 'success' | 'danger' | 'warning' | 'info';

interface ToastMessage {
  id: number;
  message: string;
  variant: ToastVariant;
}

let nextId = 0;

const TOAST_DURATION_MS = 5000;

const variantConfig: Record<ToastVariant, { icon: string }> = {
  success: { icon: 'bi-check-circle-fill' },
  danger: { icon: 'bi-exclamation-triangle-fill' },
  warning: { icon: 'bi-exclamation-circle-fill' },
  info: { icon: 'bi-info-circle-fill' },
};

function ToastItem({ toast, onRemove }: { toast: ToastMessage; onRemove: (id: number) => void }) {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    requestAnimationFrame(() => setVisible(true));
    const timer = setTimeout(() => {
      setVisible(false);
      setTimeout(() => onRemove(toast.id), 300);
    }, TOAST_DURATION_MS);
    return () => clearTimeout(timer);
  }, [toast.id, onRemove]);

  const handleClose = () => {
    setVisible(false);
    setTimeout(() => onRemove(toast.id), 300);
  };

  const config = variantConfig[toast.variant];

  return (
    <div
      className={`toast align-items-center text-bg-${toast.variant} border-0 ${visible ? 'show' : ''}`}
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
      style={{
        transition: 'opacity 0.3s ease-in-out',
        opacity: visible ? 1 : 0,
      }}
    >
      <div className="d-flex">
        <div className="toast-body d-flex align-items-center gap-2">
          <i className={`bi ${config.icon}`} aria-hidden="true"></i>
          {toast.message}
        </div>
        <button
          type="button"
          className="btn-close btn-close-white me-2 m-auto"
          onClick={handleClose}
          aria-label="Close"
        ></button>
      </div>
    </div>
  );
}

export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = useState<ToastMessage[]>([]);

  const removeToast = useCallback((id: number) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const showToast = useCallback((message: string, variant: ToastVariant = 'danger') => {
    const id = nextId++;
    setToasts((prev) => [...prev, { id, message, variant }]);
  }, []);

  return (
    <ToastContext value={{ showToast }}>
      {children}
      {createPortal(
        <div
          className="toast-container position-fixed p-3 toast-position"
          style={{ zIndex: 1090 }}
          aria-label="Notifications"
        >
          {toasts.map((toast) => (
            <ToastItem key={toast.id} toast={toast} onRemove={removeToast} />
          ))}
        </div>,
        document.body
      )}
    </ToastContext>
  );
}
