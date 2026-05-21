import * as React from 'react';
import { cn } from '@/lib/utils';

export const Label: React.FC<React.LabelHTMLAttributes<HTMLLabelElement>> = ({ className, ...props }) => (
  <label className={cn('mb-1.5 block text-xs font-semibold text-slate-700', className)} {...props} />
);
