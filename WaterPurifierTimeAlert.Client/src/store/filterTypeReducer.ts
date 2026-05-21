import type { UnknownAction } from '@reduxjs/toolkit';
import {
  FILTER_TYPE_FAILED,
  FILTER_TYPE_LOADED,
  FILTER_TYPE_LOADING,
  FILTER_TYPE_PAGE,
  FILTER_TYPE_SEARCH,
  type FailedAction,
  type FilterTypeState,
  type LoadedAction,
  type PageAction,
  type SearchAction,
} from './filterTypeTypes';

const initialState: FilterTypeState = {
  items: [],
  loading: false,
  error: null,
  query: '',
  currentPage: 1,
  itemSize: 10,
};

export function filterTypeReducer(
  state: FilterTypeState = initialState,
  action: UnknownAction,
): FilterTypeState {
  switch (action.type) {
    case FILTER_TYPE_LOADING:
      return { ...state, loading: true, error: null };
    case FILTER_TYPE_LOADED:
      return { ...state, loading: false, items: (action as LoadedAction).payload };
    case FILTER_TYPE_FAILED:
      return { ...state, loading: false, error: (action as FailedAction).payload };
    case FILTER_TYPE_SEARCH:
      return { ...state, query: (action as SearchAction).payload, currentPage: 1 };
    case FILTER_TYPE_PAGE: {
      const { currentPage, itemSize } = (action as PageAction).payload;
      return { ...state, currentPage, itemSize };
    }
    default:
      return state;
  }
}
