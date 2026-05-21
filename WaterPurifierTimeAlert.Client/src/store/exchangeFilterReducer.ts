import type { UnknownAction } from '@reduxjs/toolkit';
import {
  EXCHANGE_FILTER_FAILED,
  EXCHANGE_FILTER_LOADED,
  EXCHANGE_FILTER_LOADING,
  EXCHANGE_FILTER_PAGE,
  EXCHANGE_FILTER_SEARCH,
  type ExFailed,
  type ExLoaded,
  type ExPage,
  type ExSearch,
  type ExchangeFilterState,
} from './exchangeFilterTypes';

const initialState: ExchangeFilterState = {
  items: [],
  loading: false,
  error: null,
  query: '',
  currentPage: 1,
  itemSize: 10,
};

export function exchangeFilterReducer(
  state: ExchangeFilterState = initialState,
  action: UnknownAction,
): ExchangeFilterState {
  switch (action.type) {
    case EXCHANGE_FILTER_LOADING:
      return { ...state, loading: true, error: null };
    case EXCHANGE_FILTER_LOADED:
      return { ...state, loading: false, items: (action as ExLoaded).payload };
    case EXCHANGE_FILTER_FAILED:
      return { ...state, loading: false, error: (action as ExFailed).payload };
    case EXCHANGE_FILTER_SEARCH:
      return { ...state, query: (action as ExSearch).payload, currentPage: 1 };
    case EXCHANGE_FILTER_PAGE: {
      const { currentPage, itemSize } = (action as ExPage).payload;
      return { ...state, currentPage, itemSize };
    }
    default:
      return state;
  }
}
