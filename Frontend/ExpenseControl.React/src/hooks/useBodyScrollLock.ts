import { useEffect, useRef } from 'react';

export function useBodyScrollLock(isOpen: boolean): void {
  const scrollYRef = useRef(0);

  useEffect(() => {
    if (!isOpen) return;

    scrollYRef.current = window.scrollY;
    document.body.style.top = `-${scrollYRef.current}px`;
    document.body.classList.add('modal-open');

    return () => {
      document.body.classList.remove('modal-open');
      document.body.style.top = '';
      window.scrollTo(0, scrollYRef.current);
    };
  }, [isOpen]);
}
