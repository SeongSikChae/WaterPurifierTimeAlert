import type { FilterType, SearchQuery } from '@/types/filterType';

const BASE = '/api/FilterType';

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

export const filterTypeClient = {
  list: (query: SearchQuery) => request<FilterType[]>(`${BASE}/GetFilterTypes`, query, 'POST') as Promise<FilterType[]>,
  create: (filterType: FilterType) => request<void>(`${BASE}/CreateFilterType`, filterType, 'PUT'),
  update: (filterType: FilterType) => request<void>(`${BASE}/UpdateFilterType`, filterType, 'PUT'),
  remove: (filterType: FilterType) => request<void>(`${BASE}/DeleteFilterType`, filterType, 'PUT'),
};
