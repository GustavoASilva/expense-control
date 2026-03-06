import { useState, useCallback, useRef } from 'react';
import { createPortal } from 'react-dom';
import { ConfirmContext, ConfirmOptions } from '../hooks/useConfirm';

export function ConfirmDialogProvider({ children }: { children: React.ReactNode }) {
  const [options, setOptions] = useState<ConfirmOptions | null>(null);
  const resolveRef = useRef<((value: boolean) => void) | null>(null);

  const confirm = useCallback((opts: ConfirmOptions): Promise<boolean> => {
    setOptions(opts);
    return new Promise<boolean>((resolve) => {
      resolveRef.current = resolve;
    });
  }, []);

  const handleConfirm = () => {
    resolveRef.current?.(true);
    resolveRef.current = null;
    setOptions(null);
  };

  const handleCancel = () => {
    resolveRef.current?.(false);
    resolveRef.current = null;
    setOptions(null);
  };

  const variant = options?.variant ?? 'danger';

  return (
    <ConfirmContext value={{ confirm }}>
      {children}
      {options &&
        createPortal(
          <div
            className="modal show d-block"
            tabIndex={-1}
            style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}
            onClick={handleCancel}
            role="dialog"
            aria-modal="true"
            aria-labelledby="confirm-dialog-title"
          >
            <div
              className="modal-dialog modal-dialog-centered modal-sm"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="modal-content">
                <div className="modal-header">
                  <h5 className="modal-title" id="confirm-dialog-title">
                    {options.title ?? 'Confirm'}
                  </h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={handleCancel}
                    aria-label="Close"
                  ></button>
                </div>
                <div className="modal-body">
                  <p className="mb-0">{options.message}</p>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-outline-secondary"
                    onClick={handleCancel}
                  >
                    {options.cancelText ?? 'Cancel'}
                  </button>
                  <button
                    type="button"
                    className={`btn btn-${variant}`}
                    onClick={handleConfirm}
                  >
                    {options.confirmText ?? 'Confirm'}
                  </button>
                </div>
              </div>
            </div>
          </div>,
          document.body
        )}
    </ConfirmContext>
  );
}
