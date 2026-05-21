import { exchangeFilterClient } from '@/api/exchangeFilterClient';
import type { ExchangeFilter } from '@/types/filterType';
import type { AppThunk } from './index';
import {
  EXCHANGE_FILTER_FAILED,
  EXCHANGE_FILTER_LOADED,
  EXCHANGE_FILTER_LOADING,
  EXCHANGE_FILTER_PAGE,
  EXCHANGE_FILTER_SEARCH,
  type ExchangeFilterAction,
} from './exchangeFilterTypes';

export const setExchangeQuery = (query: string): ExchangeFilterAction => ({
  type: EXCHANGE_FILTER_SEARCH,
  payload: query,
});

export const setExchangePage = (currentPage: number, itemSize: number): ExchangeFilterAction => ({
  type: EXCHANGE_FILTER_PAGE,
  payload: { currentPage, itemSize },
});

export function fetchExchangeFilters(): AppThunk<Promise<void>> {
  return async (dispatch, getState) => {
    dispatch({ type: EXCHANGE_FILTER_LOADING });
    try {
      const { query, currentPage, itemSize } = getState().exchangeFilter;
      const items = await exchangeFilterClient.list({
        query: query || null,
        pagination: { currentPage, itemSize },
      });
      dispatch({ type: EXCHANGE_FILTER_LOADED, payload: items });
    } catch (error) {
      dispatch({ type: EXCHANGE_FILTER_FAILED, payload: (error as Error).message });
    }
  };
}

export function createExchangeFilter(item: ExchangeFilter): AppThunk<Promise<void>> {
  return async (dispatch) => {
    await exchangeFilterClient.create(item);
    await dispatch(fetchExchangeFilters());
  };
}

export function updateExchangeFilter(item: ExchangeFilter): AppThunk<Promise<void>> {
  return async (dispatch) => {
    await exchangeFilterClient.update(item);
    await dispatch(fetchExchangeFilters());
  };
}

export function deleteExchangeFilter(item: ExchangeFilter): AppThunk<Promise<void>> {
  return async (dispatch) => {
    await exchangeFilterClient.remove(item);
    await dispatch(fetchExchangeFilters());
  };
}
