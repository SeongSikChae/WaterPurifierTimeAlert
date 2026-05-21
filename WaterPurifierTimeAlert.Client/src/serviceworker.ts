// Web Push 알림 수신용 서비스 워커 (추후 PushController와 연동)
declare const self: ServiceWorkerGlobalScope;
export {};

self.addEventListener('push', (event: PushEvent) => {
  const data = event.data?.json?.() ?? { title: '알림', body: '' };
  event.waitUntil(
    self.registration.showNotification(data.title ?? '알림', {
      body: data.body ?? '',
      icon: '/icon-192.png',
    }),
  );
});

self.addEventListener('notificationclick', (event: NotificationEvent) => {
  event.notification.close();
  event.waitUntil(self.clients.openWindow('/'));
});
