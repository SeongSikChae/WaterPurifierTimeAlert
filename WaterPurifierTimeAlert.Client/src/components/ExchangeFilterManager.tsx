import * as React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Pencil, Plus, RefreshCcw, Search, Trash2 } from 'lucide-react';
import type { AppDispatch, RootState } from '@/store';
import {
  createExchangeFilter,
  deleteExchangeFilter,
  fetchExchangeFilters,
  setExchangePage,
  setExchangeQuery,
  updateExchangeFilter,
} from '@/store/exchangeFilterActions';
import { fetchFilterTypes } from '@/store/filterTypeActions';
import type { ExchangeFilter } from '@/types/filterType';
import { formatDate } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { TBody, TD, TH, THead, TR, Table } from '@/components/ui/table';

interface EditState {
  open: boolean;
  mode: 'create' | 'edit';
  filterName: string;
  lastExchnageDate: string;
  error: string | null;
  saving: boolean;
}

const emptyEdit: EditState = {
  open: false,
  mode: 'create',
  filterName: '',
  lastExchnageDate: '',
  error: null,
  saving: false,
};

function toInputDate(value: string): string {
  if (!value) return '';
  const d = new Date(value);
  if (isNaN(d.getTime())) return '';
  return d.toISOString().slice(0, 10);
}

function nextExchangeBadge(item: ExchangeFilter): React.ReactElement {
  if (!item.nextExchnageDate) return <span className="text-slate-400">-</span>;
  const next = new Date(item.nextExchnageDate).getTime();
  const now = Date.now();
  const diffDays = Math.floor((next - now) / (1000 * 60 * 60 * 24));
  if (diffDays < 0) {
    return (
      <span className="rounded-full bg-rose-100 px-2 py-0.5 text-xs font-semibold text-rose-700">
        기한 초과 ({formatDate(item.nextExchnageDate)})
      </span>
    );
  }
  if (diffDays < 14) {
    return (
      <span className="rounded-full bg-amber-100 px-2 py-0.5 text-xs font-semibold text-amber-700">
        {diffDays}일 남음 ({formatDate(item.nextExchnageDate)})
      </span>
    );
  }
  return (
    <span className="rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-semibold text-emerald-700">
      {formatDate(item.nextExchnageDate)}
    </span>
  );
}

export const ExchangeFilterManager: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { items, loading, error, query, currentPage, itemSize } = useSelector(
    (state: RootState) => state.exchangeFilter,
  );
  const filterTypes = useSelector((state: RootState) => state.filterType.items);

  const [searchInput, setSearchInput] = React.useState(query);
  const [edit, setEdit] = React.useState<EditState>(emptyEdit);
  const [confirmDelete, setConfirmDelete] = React.useState<ExchangeFilter | null>(null);

  React.useEffect(() => {
    dispatch(fetchExchangeFilters());
  }, [dispatch, query, currentPage, itemSize]);

  React.useEffect(() => {
    if (filterTypes.length === 0) dispatch(fetchFilterTypes());
  }, [dispatch, filterTypes.length]);

  const handleSearch = (event: React.FormEvent) => {
    event.preventDefault();
    dispatch(setExchangeQuery(searchInput.trim()));
  };

  const openCreate = () =>
    setEdit({
      ...emptyEdit,
      open: true,
      mode: 'create',
      lastExchnageDate: new Date().toISOString().slice(0, 10),
    });

  const openEdit = (item: ExchangeFilter) =>
    setEdit({
      open: true,
      mode: 'edit',
      filterName: item.filterName,
      lastExchnageDate: toInputDate(item.lastExchnageDate),
      error: null,
      saving: false,
    });

  const closeEdit = () => setEdit(emptyEdit);

  const submitEdit = async () => {
    if (!edit.filterName) return setEdit((s) => ({ ...s, error: '필터를 선택하세요.' }));
    if (!edit.lastExchnageDate) return setEdit((s) => ({ ...s, error: '교체 날짜를 선택하세요.' }));

    setEdit((s) => ({ ...s, saving: true, error: null }));
    try {
      const payload: ExchangeFilter = {
        filterName: edit.filterName,
        lastExchnageDate: new Date(edit.lastExchnageDate).toISOString(),
      };
      if (edit.mode === 'create') await dispatch(createExchangeFilter(payload));
      else await dispatch(updateExchangeFilter(payload));
      closeEdit();
    } catch (e) {
      setEdit((s) => ({ ...s, saving: false, error: (e as Error).message }));
    }
  };

  const submitDelete = async () => {
    if (!confirmDelete) return;
    try {
      await dispatch(deleteExchangeFilter(confirmDelete));
    } finally {
      setConfirmDelete(null);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>필터 교체 이력 관리</CardTitle>
        <div className="flex items-center gap-2">
          <Button size="sm" variant="outline" onClick={() => dispatch(fetchExchangeFilters())} disabled={loading}>
            <RefreshCcw className="h-4 w-4" />
            새로고침
          </Button>
          <Button size="sm" onClick={openCreate}>
            <Plus className="h-4 w-4" />
            추가
          </Button>
        </div>
      </CardHeader>
      <CardBody className="space-y-4">
        <form onSubmit={handleSearch} className="flex items-center gap-2">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <Input
              placeholder="필터 이름 검색"
              className="pl-9"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
          </div>
          <Button type="submit" variant="secondary" size="sm">
            검색
          </Button>
        </form>

        {error ? (
          <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">{error}</div>
        ) : null}

        <Table>
          <THead>
            <TR>
              <TH>필터 이름</TH>
              <TH>마지막 교체일</TH>
              <TH>다음 교체일</TH>
              <TH className="w-32 text-right">동작</TH>
            </TR>
          </THead>
          <TBody>
            {loading ? (
              <TR>
                <TD colSpan={4} className="py-8 text-center text-slate-400">
                  불러오는 중...
                </TD>
              </TR>
            ) : items.length === 0 ? (
              <TR>
                <TD colSpan={4} className="py-8 text-center text-slate-400">
                  데이터가 없습니다.
                </TD>
              </TR>
            ) : (
              items.map((item) => (
                <TR key={item.filterName}>
                  <TD className="font-semibold text-slate-800">{item.filterName}</TD>
                  <TD>{formatDate(item.lastExchnageDate)}</TD>
                  <TD>{nextExchangeBadge(item)}</TD>
                  <TD>
                    <div className="flex justify-end gap-1">
                      <Button size="icon" variant="ghost" onClick={() => openEdit(item)} aria-label="수정">
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        size="icon"
                        variant="ghost"
                        className="text-sb-danger hover:bg-rose-50"
                        onClick={() => setConfirmDelete(item)}
                        aria-label="삭제"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TD>
                </TR>
              ))
            )}
          </TBody>
        </Table>

        <div className="flex items-center justify-between text-sm text-slate-600">
          <div>
            페이지 크기:{' '}
            <select
              className="rounded border border-slate-300 bg-white px-2 py-1"
              value={itemSize}
              onChange={(e) => dispatch(setExchangePage(1, Number(e.target.value)))}
            >
              <option value={10}>10</option>
              <option value={20}>20</option>
              <option value={50}>50</option>
            </select>
          </div>
          <div className="flex items-center gap-2">
            <Button
              size="sm"
              variant="outline"
              onClick={() => dispatch(setExchangePage(Math.max(1, currentPage - 1), itemSize))}
              disabled={currentPage <= 1 || loading}
            >
              이전
            </Button>
            <span className="px-2">페이지 {currentPage}</span>
            <Button
              size="sm"
              variant="outline"
              onClick={() => dispatch(setExchangePage(currentPage + 1, itemSize))}
              disabled={loading || items.length < itemSize}
            >
              다음
            </Button>
          </div>
        </div>
      </CardBody>

      <Dialog
        open={edit.open}
        onClose={closeEdit}
        title={edit.mode === 'create' ? '교체 이력 추가' : '교체 이력 수정'}
        footer={
          <>
            <Button variant="outline" size="sm" onClick={closeEdit} disabled={edit.saving}>
              취소
            </Button>
            <Button size="sm" onClick={submitEdit} disabled={edit.saving}>
              {edit.saving ? '저장 중...' : '저장'}
            </Button>
          </>
        }
      >
        <div className="space-y-3">
          <div>
            <Label>필터 종류</Label>
            {edit.mode === 'create' ? (
              <select
                className="h-10 w-full rounded-md border border-slate-300 bg-white px-3 text-sm"
                value={edit.filterName}
                onChange={(e) => setEdit((s) => ({ ...s, filterName: e.target.value }))}
              >
                <option value="">선택...</option>
                {filterTypes.map((t) => (
                  <option key={t.name} value={t.name}>
                    {t.name} ({t.expireTime})
                  </option>
                ))}
              </select>
            ) : (
              <Input value={edit.filterName} disabled />
            )}
          </div>
          <div>
            <Label>마지막 교체일</Label>
            <Input
              type="date"
              value={edit.lastExchnageDate}
              onChange={(e) => setEdit((s) => ({ ...s, lastExchnageDate: e.target.value }))}
            />
          </div>
          {edit.error ? <p className="text-sm text-rose-600">{edit.error}</p> : null}
        </div>
      </Dialog>

      <Dialog
        open={!!confirmDelete}
        onClose={() => setConfirmDelete(null)}
        title="삭제 확인"
        footer={
          <>
            <Button variant="outline" size="sm" onClick={() => setConfirmDelete(null)}>
              취소
            </Button>
            <Button variant="danger" size="sm" onClick={submitDelete}>
              삭제
            </Button>
          </>
        }
      >
        <p className="text-sm text-slate-700">
          <strong>{confirmDelete?.filterName}</strong> 교체 이력을 삭제하시겠습니까?
        </p>
      </Dialog>
    </Card>
  );
};
