import type { UnknownAction } from '@reduxjs/toolkit';
import type { ExchangeFilter } from '@/types/filterType';

export const EXCHANGE_FILTER_LOADING = 'exchangeFilter/LOADING';
export const EXCHANGE_FILTER_LOADED = 'exchangeFilter/LOADED';
export const EXCHANGE_FILTER_FAILED = 'exchangeFilter/FAILED';
export const EXCHANGE_FILTER_SEARCH = 'exchangeFilter/SEARCH';
export const EXCHANGE_FILTER_PAGE = 'exchangeFilter/PAGE';

export interface ExchangeFilterState {
  items: ExchangeFilter[];
  loading: boolean;
  error: string | null;
  query: string;
  currentPage: number;
  itemSize: number;
}

export interface ExLoading extends UnknownAction { type: typeof EXCHANGE_FILTER_LOADING; }
export interface ExLoaded extends UnknownAction { type: typeof EXCHANGE_FILTER_LOADED; payload: ExchangeFilter[]; }
export interface ExFailed extends UnknownAction { type: typeof EXCHANGE_FILTER_FAILED; payload: string; }
export interface ExSearch extends UnknownAction { type: typeof EXCHANGE_FILTER_SEARCH; payload: string; }
export interface ExPage extends UnknownAction { type: typeof EXCHANGE_FILTER_PAGE; payload: { currentPage: number; itemSize: number }; }

export type ExchangeFilterAction = ExLoading | ExLoaded | ExFailed | ExSearch | ExPage;
