import * as React from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Pencil, Plus, RefreshCcw, Search, Trash2 } from 'lucide-react';
import type { AppDispatch, RootState } from '@/store';
import {
  createFilterType,
  deleteFilterType,
  fetchFilterTypes,
  setFilterTypePage,
  setFilterTypeQuery,
  updateFilterType,
} from '@/store/filterTypeActions';
import type { FilterType } from '@/types/filterType';
import { EXPIRE_TIME_HELP, isValidExpireTimeExpression } from '@/lib/expireTimeExpression';
import { Button } from '@/components/ui/button';
import { Card, CardBody, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { TBody, TD, TH, THead, TR, Table } from '@/components/ui/table';

interface EditState {
  open: boolean;
  mode: 'create' | 'edit';
  original: FilterType | null;
  name: string;
  expireTime: string;
  error: string | null;
  saving: boolean;
}

const emptyEdit: EditState = {
  open: false,
  mode: 'create',
  original: null,
  name: '',
  expireTime: '',
  error: null,
  saving: false,
};

export const FilterTypeManager: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { items, loading, error, query, currentPage, itemSize } = useSelector(
    (state: RootState) => state.filterType,
  );

  const [searchInput, setSearchInput] = React.useState(query);
  const [edit, setEdit] = React.useState<EditState>(emptyEdit);
  const [confirmDelete, setConfirmDelete] = React.useState<FilterType | null>(null);

  React.useEffect(() => {
    dispatch(fetchFilterTypes());
  }, [dispatch, query, currentPage, itemSize]);

  const handleSearch = (event: React.FormEvent) => {
    event.preventDefault();
    dispatch(setFilterTypeQuery(searchInput.trim()));
  };

  const openCreate = () =>
    setEdit({ ...emptyEdit, open: true, mode: 'create' });

  const openEdit = (item: FilterType) =>
    setEdit({
      open: true,
      mode: 'edit',
      original: item,
      name: item.name,
      expireTime: item.expireTime,
      error: null,
      saving: false,
    });

  const closeEdit = () => setEdit(emptyEdit);

  const submitEdit = async () => {
    const name = edit.name.trim();
    const expireTime = edit.expireTime.trim();
    if (!name) return setEdit((e) => ({ ...e, error: '필터 이름을 입력하세요.' }));
    if (!isValidExpireTimeExpression(expireTime))
      return setEdit((e) => ({ ...e, error: `만료 표현식이 올바르지 않습니다. ${EXPIRE_TIME_HELP}` }));

    setEdit((e) => ({ ...e, saving: true, error: null }));
    try {
      if (edit.mode === 'create') {
        await dispatch(createFilterType({ name, expireTime }));
      } else {
        await dispatch(updateFilterType({ name, expireTime }));
      }
      closeEdit();
    } catch (e) {
      setEdit((s) => ({ ...s, saving: false, error: (e as Error).message }));
    }
  };

  const submitDelete = async () => {
    if (!confirmDelete) return;
    try {
      await dispatch(deleteFilterType(confirmDelete));
    } finally {
      setConfirmDelete(null);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>필터 종류 관리</CardTitle>
        <div className="flex items-center gap-2">
          <Button size="sm" variant="outline" onClick={() => dispatch(fetchFilterTypes())} disabled={loading}>
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
              <TH className="w-1/2">이름</TH>
              <TH>만료 표현식</TH>
              <TH className="w-32 text-right">동작</TH>
            </TR>
          </THead>
          <TBody>
            {loading ? (
              <TR>
                <TD colSpan={3} className="py-8 text-center text-slate-400">
                  불러오는 중...
                </TD>
              </TR>
            ) : items.length === 0 ? (
              <TR>
                <TD colSpan={3} className="py-8 text-center text-slate-400">
                  데이터가 없습니다.
                </TD>
              </TR>
            ) : (
              items.map((item) => (
                <TR key={item.name}>
                  <TD className="font-semibold text-slate-800">{item.name}</TD>
                  <TD>
                    <code className="rounded bg-slate-100 px-2 py-0.5 text-xs">{item.expireTime}</code>
                  </TD>
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
              onChange={(e) => dispatch(setFilterTypePage(1, Number(e.target.value)))}
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
              onClick={() => dispatch(setFilterTypePage(Math.max(1, currentPage - 1), itemSize))}
              disabled={currentPage <= 1 || loading}
            >
              이전
            </Button>
            <span className="px-2">페이지 {currentPage}</span>
            <Button
              size="sm"
              variant="outline"
              onClick={() => dispatch(setFilterTypePage(currentPage + 1, itemSize))}
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
        title={edit.mode === 'create' ? '필터 종류 추가' : '필터 종류 수정'}
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
            <Label>이름</Label>
            <Input
              value={edit.name}
              disabled={edit.mode === 'edit'}
              onChange={(e) => setEdit((s) => ({ ...s, name: e.target.value }))}
              placeholder="예: PP필터"
              maxLength={20}
            />
          </div>
          <div>
            <Label>만료 표현식</Label>
            <Input
              value={edit.expireTime}
              onChange={(e) => setEdit((s) => ({ ...s, expireTime: e.target.value }))}
              placeholder="예: +1y@d"
              maxLength={10}
            />
            <p className="mt-1 text-xs text-slate-500">{EXPIRE_TIME_HELP}</p>
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
          <strong>{confirmDelete?.name}</strong> 필터 종류를 삭제하시겠습니까?
        </p>
      </Dialog>
    </Card>
  );
};
