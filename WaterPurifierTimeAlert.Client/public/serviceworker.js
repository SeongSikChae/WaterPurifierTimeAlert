self.addEventListener('push', (event) => {
  console.log('[SW] push 이벤트 도착, data:', event.data ? event.data.text() : '(empty)');
  let data = { title: '알림', body: '' };
  try {
    if (event.data) data = event.data.json();
  } catch (e) {
    console.warn('[SW] payload JSON 파싱 실패:', e);
    if (event.data) data = { title: '알림', body: event.data.text() };
  }

  const title = data.title || '알림';
  const options = {
    body: data.body || '',
    icon: '/avatar.png',
    tag: data.tag || undefined,
    renotify: !!data.tag,
    data: data,
  };

  event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  event.waitUntil(
    self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clients) => {
      for (const client of clients) {
        if ('focus' in client) return client.focus();
      }
      if (self.clients.openWindow) return self.clients.openWindow('/');
    })
  );
});
