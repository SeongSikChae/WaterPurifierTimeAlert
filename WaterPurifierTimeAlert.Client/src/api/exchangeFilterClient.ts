import type { ExchangeFilter, SearchQuery } from '@/types/filterType';

const BASE = '/api/ExchangeFilter';

async function request<T>(url: string, body: unknown, method: 'POST' | 'PUT'): Promise<T | void> {
  const response = await fetch(url, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
    credentials: 'include',
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `${response.status} ${response.statusText}`);
  }
  const contentType = response.headers.get('content-type') || '';
  if (contentType.indexOf('application/json') !== -1) {
    return (await response.json()) as T;
  }
  return;
}

export const exchangeFilterClient = {
  list: (query: SearchQuery) =>
    request<ExchangeFilter[]>(`${BASE}/GetExchangeFilters`, query, 'POST') as Promise<ExchangeFilter[]>,
  create: (item: ExchangeFilter) => request<void>(`${BASE}/CreateExchangeFilter`, item, 'PUT'),
  update: (item: ExchangeFilter) => request<void>(`${BASE}/UpdateExchangeFilter`, item, 'PUT'),
  remove: (item: ExchangeFilter) => request<void>(`${BASE}/DeleteExchangeFilter`, item, 'PUT'),
};
