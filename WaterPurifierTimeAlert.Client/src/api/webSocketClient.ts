import type { ExchangeFilter } from '@/types/filterType';

export interface WebSocketClientOptions {
  onMessage?: (data: ExchangeFilter) => void;
  onOpen?: () => void;
  onClose?: (event: CloseEvent) => void;
  onError?: (event: Event) => void;
  reconnectDelayMs?: number;
}

export interface WebSocketClient {
  close: () => void;
}

function buildWebSocketUrl(): string {
  const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
  return `${protocol}//${window.location.host}/ws`;
}

export async function requestNotificationPermission(): Promise<NotificationPermission> {
  if (typeof Notification === 'undefined') return 'denied';
  if (Notification.permission === 'granted' || Notification.permission === 'denied') {
    return Notification.permission;
  }
  try {
    return await Notification.requestPermission();
  } catch {
    return 'denied';
  }
}

function showFilterNotification(data: ExchangeFilter): void {
  if (typeof Notification === 'undefined' || Notification.permission !== 'granted') return;

  const title = '필터 교체 알림';
  const bodyLines: string[] = [];
  if (data.filterName) bodyLines.push(`필터: ${data.filterName}`);
  if (data.nextExchnageDate) bodyLines.push(`다음 교체일: ${data.nextExchnageDate}`);
  else if (data.lastExchnageDate) bodyLines.push(`최근 교체일: ${data.lastExchnageDate}`);

  const tag = data.filterName ?? '';
  const options: NotificationOptions = {
    body: bodyLines.join('\n'),
    icon: '/avatar.png',
  };
  if (tag) {
    options.tag = tag;
    (options as NotificationOptions & { renotify?: boolean }).renotify = true;
  }

  try {
    new Notification(title, options);
  } catch (err) {
    console.error('[WebSocket] 알림 표시 실패:', err);
  }
}

export function connectWebSocket(options: WebSocketClientOptions = {}): WebSocketClient {
  const { onMessage, onOpen, onClose, onError, reconnectDelayMs = 3000 } = options;

  let socket: WebSocket | null = null;
  let reconnectTimer: number | null = null;
  let closedByUser = false;

  const connect = (): void => {
    const url = buildWebSocketUrl();
    socket = new WebSocket(url);

    socket.onopen = () => {
      onOpen?.();
    };

    socket.onmessage = (event: MessageEvent<string>) => {
      try {
        const data = JSON.parse(event.data) as ExchangeFilter;
        showFilterNotification(data);
        onMessage?.(data);
      } catch (err) {
        console.error('[WebSocket] 메시지 파싱 실패:', err);
      }
    };

    socket.onerror = (event) => {
      onError?.(event);
    };

    socket.onclose = (event) => {
      onClose?.(event);
      socket = null;
      if (!closedByUser) {
        reconnectTimer = window.setTimeout(connect, reconnectDelayMs);
      }
    };
  };

  connect();

  return {
    close: () => {
      closedByUser = true;
      if (reconnectTimer !== null) {
        window.clearTimeout(reconnectTimer);
        reconnectTimer = null;
      }
      socket?.close();
    },
  };
}
