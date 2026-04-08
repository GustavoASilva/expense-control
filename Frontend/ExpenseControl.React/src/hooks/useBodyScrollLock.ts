import { useEffect } from 'react';

export function useBodyScrollLock(isOpen: boolean): void {
  useEffect(() => {
    if (isOpen) {
      document.body.classList.add('modal-open');
    }
    return () => {
      document.body.classList.remove('modal-open');
    };
  }, [isOpen]);
}
