import { configureStore, type ThunkAction, type UnknownAction } from '@reduxjs/toolkit';
import { useDispatch, useSelector, type TypedUseSelectorHook } from 'react-redux';
import { filterTypeReducer } from './filterTypeReducer';
import { exchangeFilterReducer } from './exchangeFilterReducer';

export const store = configureStore({
  reducer: {
    filterType: filterTypeReducer,
    exchangeFilter: exchangeFilterReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

export type AppThunk<R = void> = ThunkAction<R, RootState, unknown, UnknownAction>;

export const useAppDispatch: () => AppDispatch = useDispatch;
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
