// User types
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  createdAt: string;
  isActive: boolean;
}

// Auth types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  email: string;
  firstName: string;
  lastName: string;
  expiresAt: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

// File types
export interface FileRecord {
  id: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  fileHash: string;
  userId: string;
  status: FileStatus;
  uploadedAt: string;
  processedAt?: string;
  downloadUrl?: string;
  metadata?: Record<string, string>;
}

export type FileStatus = 
  | 'pending'
  | 'uploading'
  | 'processing'
  | 'completed'
  | 'failed'
  | 'deleted';

export interface FileUploadProgress {
  fileId: string;
  fileName: string;
  progress: number;
  status: FileStatus;
  message?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface FileSearchParams {
  query?: string;
  contentType?: string;
  status?: FileStatus;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}

// API Error types
export interface ApiError {
  message: string;
  code?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
}

// Form validation types
export interface ValidationError {
  field: string;
  message: string;
}

// Component prop types
export interface FileListProps {
  files: FileRecord[];
  onDelete: (fileId: string) => void;
  onDownload: (fileId: string) => void;
  isLoading?: boolean;
}

export interface FileUploadProps {
  onUpload: (file: File) => Promise<void>;
  accept?: string[];
  maxSize?: number;
  disabled?: boolean;
}

export interface UploadProgressProps {
  uploads: FileUploadProgress[];
}
