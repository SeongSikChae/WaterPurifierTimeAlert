import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { cn } from '@/lib/utils';

export interface DialogProps {
  open: boolean;
  onClose: () => void;
  title?: React.ReactNode;
  children?: React.ReactNode;
  footer?: React.ReactNode;
  className?: string;
}

export const Dialog: React.FC<DialogProps> = ({ open, onClose, title, children, footer, className }) => {
  React.useEffect(() => {
    if (!open) return;
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) return null;

  return ReactDOM.createPortal(
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-slate-900/50" onClick={onClose} />
      <div
        role="dialog"
        aria-modal="true"
        className={cn(
          'sb-card-shadow relative z-10 w-full max-w-md rounded-lg border border-slate-200 bg-white',
          className,
        )}
      >
        {title ? (
          <div className="border-b border-slate-200 px-5 py-3">
            <h2 className="text-sb-primary text-sm font-bold">{title}</h2>
          </div>
        ) : null}
        <div className="p-5">{children}</div>
        {footer ? (
          <div className="flex justify-end gap-2 border-t border-slate-200 bg-slate-50 px-5 py-3">{footer}</div>
        ) : null}
      </div>
    </div>,
    document.body,
  );
};
