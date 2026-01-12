import { create } from 'zustand';
import axios from 'axios';

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
      const response = await axios.get(API_URL);
      set({ files: response.data.files || response.data, isLoading: false });
    } catch (error: any) {
      set({ error: error.message, isLoading: false });
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

      await axios.post(`${API_URL}/upload`, formData, {
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
      await get().fetchFiles();
    } catch (error: any) {
      get().setUploadProgress(tempFileId, 0, 'error');
      set({ error: error.message });
    }
  },

  deleteFile: async (fileId: string) => {
    try {
      await axios.delete(`${API_URL}/${fileId}`);
      set((state) => ({
        files: state.files.filter((f) => f.fileId !== fileId),
      }));
    } catch (error: any) {
      set({ error: error.message });
    }
  },

  getDownloadUrl: async (fileId: string) => {
    const response = await axios.get(`${API_URL}/${fileId}/download`);
    return response.data.downloadUrl;
  },

  setUploadProgress: (fileId: string, progress: number, status: 'uploading' | 'completed' | 'error') => {
    set((state) => ({
      uploadProgress: state.uploadProgress.map((p) =>
        p.fileId === fileId ? { ...p, progress, status } : p
      ),
    }));
  },
}));
