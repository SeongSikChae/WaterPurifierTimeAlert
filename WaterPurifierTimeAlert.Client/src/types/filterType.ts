export interface FilterType {
  name: string;
  expireTime: string;
}

export interface ExchangeFilter {
  filterName: string;
  lastExchnageDate: string;
  nextExchnageDate?: string;
}

export interface Pagination {
  currentPage: number;
  itemSize: number;
}

export interface SearchQuery {
  query?: string | null;
  pagination: Pagination;
}

export interface User {
  email: string;
  notBefore: number;
  notAfter: number;
  thumbprint: string;
}
