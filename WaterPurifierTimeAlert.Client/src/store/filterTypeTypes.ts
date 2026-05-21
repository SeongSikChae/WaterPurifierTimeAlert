import type { UnknownAction } from '@reduxjs/toolkit';
import type { FilterType } from '@/types/filterType';

export const FILTER_TYPE_LOADING = 'filterType/LOADING';
export const FILTER_TYPE_LOADED = 'filterType/LOADED';
export const FILTER_TYPE_FAILED = 'filterType/FAILED';
export const FILTER_TYPE_SEARCH = 'filterType/SEARCH';
export const FILTER_TYPE_PAGE = 'filterType/PAGE';

export interface FilterTypeState {
  items: FilterType[];
  loading: boolean;
  error: string | null;
  query: string;
  currentPage: number;
  itemSize: number;
}

export interface LoadingAction extends UnknownAction { type: typeof FILTER_TYPE_LOADING; }
export interface LoadedAction extends UnknownAction { type: typeof FILTER_TYPE_LOADED; payload: FilterType[]; }
export interface FailedAction extends UnknownAction { type: typeof FILTER_TYPE_FAILED; payload: string; }
export interface SearchAction extends UnknownAction { type: typeof FILTER_TYPE_SEARCH; payload: string; }
export interface PageAction extends UnknownAction { type: typeof FILTER_TYPE_PAGE; payload: { currentPage: number; itemSize: number }; }

export type FilterTypeAction =
  | LoadingAction
  | LoadedAction
  | FailedAction
  | SearchAction
  | PageAction;
