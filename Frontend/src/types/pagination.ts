// Pagination types matching backend PagedResult<T>
export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
}

export interface ApiResult<T> {
  isSuccess: boolean;
  content: T;
  statusCode: number;
  errorMessage?: string;
}
