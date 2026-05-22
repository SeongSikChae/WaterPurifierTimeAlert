import * as React from 'react';
import { Droplets, Filter, Gauge, RefreshCcw } from 'lucide-react';
import { FilterTypeManager } from '@/components/FilterTypeManager';
import { ExchangeFilterManager } from '@/components/ExchangeFilterManager';
import { UserBadge } from '@/components/UserBadge';
import { cn } from '@/lib/utils';
import { subscribePush, unsubscribePush } from '@/api/pushClient';
import { Bell, BellOff } from 'lucide-react';

type View = 'filterType' | 'exchangeFilter';

interface NavItem {
  id: View;
  label: string;
  icon: React.ReactElement;
}

const NAV: NavItem[] = [
  { id: 'exchangeFilter', label: '교체 이력', icon: <RefreshCcw className="h-4 w-4" /> },
  { id: 'filterType', label: '필터 종류', icon: <Filter className="h-4 w-4" /> },
];

const App: React.FC = () => {
  const [view, setView] = React.useState<View>('exchangeFilter');
  const [sidebarOpen, setSidebarOpen] = React.useState(true);
  const [notifPermission, setNotifPermission] = React.useState<NotificationPermission>(
    typeof Notification !== 'undefined' ? Notification.permission : 'denied',
  );

  const handleRequestNotification = React.useCallback(async () => {
    if (notifPermission === 'granted') {
      await unsubscribePush();
      setNotifPermission(typeof Notification !== 'undefined' ? Notification.permission : 'denied');
      return;
    }
    const sub = await subscribePush();
    setNotifPermission(typeof Notification !== 'undefined' ? Notification.permission : 'denied');
    if (sub) {
      try {
        new Notification('알림이 활성화되었습니다', { body: '필터 교체 알림을 받게 됩니다.' });
      } catch {}
    }
  }, [notifPermission]);

  return (
    <div className="flex h-screen w-screen overflow-hidden">
      <aside
        className={cn(
          'sb-sidebar flex flex-col text-white transition-all duration-200',
          sidebarOpen ? 'w-56' : 'w-16',
        )}
      >
        <div className="flex h-16 items-center justify-center border-b border-white/10 px-3">
          <Droplets className="h-6 w-6" />
          {sidebarOpen && <span className="ml-2 text-sm font-extrabold tracking-wider uppercase">정수기 알림</span>}
        </div>
        <nav className="flex-1 space-y-0.5 px-2 py-3">
          {NAV.map((item) => {
            const active = item.id === view;
            return (
              <button
                key={item.id}
                onClick={() => setView(item.id)}
                className={cn(
                  'flex w-full items-center gap-3 rounded-md px-3 py-2 text-left text-sm font-semibold transition-colors',
                  active ? 'bg-white/20 text-white' : 'text-white/80 hover:bg-white/10 hover:text-white',
                )}
              >
                {item.icon}
                {sidebarOpen && <span>{item.label}</span>}
              </button>
            );
          })}
        </nav>
        <div className="border-t border-white/10 p-3 text-xs text-white/60">
          {sidebarOpen ? 'v0.1.0 · SB Admin 2 스타일' : 'v0.1'}
        </div>
      </aside>

      <div className="flex flex-1 flex-col overflow-hidden">
        <header className="flex h-16 items-center justify-between border-b border-slate-200 bg-white px-4 shadow-sm">
          <button
            onClick={() => setSidebarOpen((v) => !v)}
            className="rounded-md p-2 text-slate-600 hover:bg-slate-100"
            aria-label="사이드바 토글"
          >
            <Gauge className="h-5 w-5" />
          </button>
          <h1 className="text-sb-dark text-base font-bold">
            {NAV.find((n) => n.id === view)?.label}
          </h1>
          <div className="flex items-center gap-2">
            <button
              onClick={handleRequestNotification}
              className={cn(
                'flex items-center gap-1 rounded-md px-2 py-1 text-xs font-semibold transition-colors',
                notifPermission === 'granted'
                  ? 'bg-emerald-100 text-emerald-700'
                  : notifPermission === 'denied'
                    ? 'bg-rose-100 text-rose-700'
                    : 'bg-slate-100 text-slate-700 hover:bg-slate-200',
              )}
              title={
                notifPermission === 'granted'
                  ? '알림이 허용되었습니다'
                  : notifPermission === 'denied'
                    ? '알림이 차단되었습니다. 브라우저 설정에서 변경하세요.'
                    : '클릭하여 알림 허용'
              }
            >
              {notifPermission === 'granted' ? <Bell className="h-3 w-3" /> : <BellOff className="h-3 w-3" />}
              {notifPermission === 'granted' ? '알림 켜짐' : notifPermission === 'denied' ? '알림 차단됨' : '알림 허용'}
            </button>
            <UserBadge />
          </div>
        </header>

        <main className="flex-1 overflow-auto bg-slate-100 p-4 md:p-6">
          {view === 'filterType' ? <FilterTypeManager /> : null}
          {view === 'exchangeFilter' ? <ExchangeFilterManager /> : null}
        </main>
      </div>
    </div>
  );
};

export default App;
