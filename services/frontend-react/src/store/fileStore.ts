import { create } from 'zustand';
import api from '../services/api';

interface FileInfo {
  fileId: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  status: string;
  createdAt: string;
  description?: string;
}

interface UploadProgress {
  fileId: string;
  progress: number;
  status: 'uploading' | 'completed' | 'error';
}

interface FileState {
  files: FileInfo[];
  uploadProgress: UploadProgress[];
  isLoading: boolean;
  error: string | null;
  fetchFiles: () => Promise<void>;
  refreshFilesSilently: () => Promise<void>;
  uploadFile: (file: File, description?: string) => Promise<void>;
  deleteFile: (fileId: string) => Promise<void>;
  getDownloadUrl: (fileId: string) => Promise<string>;
  setUploadProgress: (fileId: string, progress: number, status: 'uploading' | 'completed' | 'error') => void;
}

const API_URL = '/api/files';

export const useFileStore = create<FileState>((set, get) => ({
  files: [],
  uploadProgress: [],
  isLoading: false,
  error: null,

  fetchFiles: async () => {
    set({ isLoading: true, error: null });
    try {
      const response = await api.get(API_URL);
      set({ files: response.data.files || response.data, isLoading: false });
    } catch (error: any) {
      set({ error: error.message, isLoading: false });
    }
  },

  // Silent refresh - updates files without showing loading state (for real-time updates)
  refreshFilesSilently: async () => {
    try {
      const response = await api.get(API_URL);
      set({ files: response.data.files || response.data });
    } catch (error: any) {
      // Silently fail - don't update error state for background refreshes
      console.error('Silent refresh failed:', error.message);
    }
  },

  uploadFile: async (file: File, description?: string) => {
    const tempFileId = `temp-${Date.now()}`;
    
    set((state) => ({
      uploadProgress: [
        ...state.uploadProgress,
        { fileId: tempFileId, progress: 0, status: 'uploading' },
      ],
    }));

    try {
      const formData = new FormData();
      formData.append('file', file);
      if (description) {
        formData.append('description', description);
      }

      await api.post(`${API_URL}/upload`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        onUploadProgress: (progressEvent) => {
          const progress = progressEvent.total
            ? Math.round((progressEvent.loaded * 100) / progressEvent.total)
            : 0;
          get().setUploadProgress(tempFileId, progress, 'uploading');
        },
      });

      get().setUploadProgress(tempFileId, 100, 'completed');
      
      // Refresh file list immediately after upload
      await get().fetchFiles();
      
      // Refresh again after a delay to catch status updates from processing (silent refresh)
      setTimeout(() => {
        get().refreshFilesSilently();
      }, 3000);
      
    } catch (error: any) {
      get().setUploadProgress(tempFileId, 0, 'error');
      set({ error: error.message });
    }
  },

  deleteFile: async (fileId: string) => {
    try {
      await api.delete(`${API_URL}/${fileId}`);
      set((state) => ({
        files: state.files.filter((f) => f.fileId !== fileId),
      }));
    } catch (error: any) {
      set({ error: error.message });
    }
  },

  getDownloadUrl: async (fileId: string) => {
    // Return direct download URL - the backend will stream the file
    return `${API_URL}/${fileId}/download`;
  },

  setUploadProgress: (fileId: string, progress: number, status: 'uploading' | 'completed' | 'error') => {
    set((state) => ({
      uploadProgress: state.uploadProgress.map((p) =>
        p.fileId === fileId ? { ...p, progress, status } : p
      ),
    }));
  },
}));
