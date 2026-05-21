import * as React from 'react';
import { userClient } from '@/api/userClient';
import type { User } from '@/types/filterType';
import { Dialog } from '@/components/ui/dialog';
import { cn } from '@/lib/utils';

function formatDateTime(ms: number | undefined | null): string {
  if (!ms) return '-';
  const date = new Date(ms);
  if (isNaN(date.getTime())) return '-';
  const yyyy = date.getFullYear();
  const mm = String(date.getMonth() + 1).padStart(2, '0');
  const dd = String(date.getDate()).padStart(2, '0');
  const hh = String(date.getHours()).padStart(2, '0');
  const mi = String(date.getMinutes()).padStart(2, '0');
  const ss = String(date.getSeconds()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd} ${hh}:${mi}:${ss}`;
}

interface RowProps {
  label: string;
  value: React.ReactNode;
}

const Row: React.FC<RowProps> = ({ label, value }) => (
  <div className="grid grid-cols-[110px_1fr] overflow-hidden rounded-md border border-slate-200">
    <div className="border-r border-slate-200 bg-rose-50 px-3 py-2 text-xs font-semibold text-rose-700">
      {label}
    </div>
    <div className="break-all px-3 py-2 text-sm text-slate-700">{value}</div>
  </div>
);

export const UserBadge: React.FC = () => {
  const [user, setUser] = React.useState<User | null>(null);
  const [open, setOpen] = React.useState(false);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    let aborted = false;
    setLoading(true);
    userClient
      .whoami()
      .then((value) => {
        if (!aborted) setUser(value);
      })
      .catch((e: Error) => {
        if (!aborted) setError(e.message);
      })
      .finally(() => {
        if (!aborted) setLoading(false);
      });
    return () => {
      aborted = true;
    };
  }, []);

  const isAnonymous = !user || user.email === 'Anonymous' || !user.thumbprint;

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className={cn(
          'group flex items-center gap-2 rounded-full p-0.5 pr-3 transition-colors',
          'hover:bg-slate-100 focus:outline-none focus:ring-2 focus:ring-sb-primary/40',
        )}
        aria-label="클라이언트 인증서 정보"
      >
        <span className="relative inline-flex h-9 w-9 overflow-hidden rounded-full border border-slate-200 bg-white">
          <img src="/avatar.png" alt="" className="h-full w-full object-cover" />
        </span>
        <span className="hidden text-left text-xs leading-tight md:block">
          <span className="block max-w-[180px] truncate font-semibold text-slate-700">
            {loading ? '...' : isAnonymous ? '익명' : user!.email}
          </span>
          <span className="block text-slate-400">
            {isAnonymous ? '인증서 없음' : '클라이언트 인증서'}
          </span>
        </span>
      </button>

      <Dialog open={open} onClose={() => setOpen(false)} title="클라이언트 인증서 정보">
        {loading ? (
          <div className="py-6 text-center text-sm text-slate-400">불러오는 중...</div>
        ) : error ? (
          <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
            {error}
          </div>
        ) : isAnonymous ? (
          <div className="space-y-2 text-sm text-slate-600">
            <p>현재 클라이언트 인증서로 접속되어 있지 않습니다.</p>
            <p className="text-slate-400">인증서를 등록한 뒤 다시 접속해 주세요.</p>
          </div>
        ) : (
          <div className="space-y-2">
            <Row label="Email" value={user!.email} />
            <Row label="유효기간 시작" value={formatDateTime(user!.notBefore)} />
            <Row label="유효기간 종료" value={formatDateTime(user!.notAfter)} />
            <Row
              label="지문"
              value={<code className="font-mono text-xs">{user!.thumbprint}</code>}
            />
          </div>
        )}
      </Dialog>
    </>
  );
};
