import { filterTypeClient } from '@/api/filterTypeClient';
import type { FilterType } from '@/types/filterType';
import type { AppThunk } from './index';
import {
  FILTER_TYPE_FAILED,
  FILTER_TYPE_LOADED,
  FILTER_TYPE_LOADING,
  FILTER_TYPE_PAGE,
  FILTER_TYPE_SEARCH,
  type FilterTypeAction,
} from './filterTypeTypes';

export const setFilterTypeQuery = (query: string): FilterTypeAction => ({
  type: FILTER_TYPE_SEARCH,
  payload: query,
});

export const setFilterTypePage = (currentPage: number, itemSize: number): FilterTypeAction => ({
  type: FILTER_TYPE_PAGE,
  payload: { currentPage, itemSize },
});

export function fetchFilterTypes(): AppThunk<Promise<void>> {
  return async (dispatch, getState) => {
    dispatch({ type: FILTER_TYPE_LOADING });
    try {
      const { query, currentPage, itemSize } = getState().filterType;
      const items = await filterTypeClient.list({
        query: query || null,
        pagination: { currentPage, itemSize },
      });
      dispatch({ type: FILTER_TYPE_LOADED, payload: items });
    } catch (error) {
      dispatch({ type: FILTER_TYPE_FAILED, payload: (error as Error).message });
    }
  };
}

export function createFilterType(filterType: FilterType): AppThunk<Promise<void>> {
  return async (dispatch) => {
    await filterTypeClient.create(filterType);
    await dispatch(fetchFilterTypes());
  };
}

export function updateFilterType(filterType: FilterType): AppThunk<Promise<void>> {
  return async (dispatch) => {
    await filterTypeClient.update(filterType);
    await dispatch(fetchFilterTypes());
  };
}

export function deleteFilterType(filterType: FilterType): AppThunk<Promise<void>> {
  return async (dispatch) => {
    await filterTypeClient.remove(filterType);
    await dispatch(fetchFilterTypes());
  };
}
