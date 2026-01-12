import { useEffect } from 'react';
import { useFileStore } from '../store/fileStore';
import FileUpload from '../components/FileUpload';
import FileList from '../components/FileList';
import UploadProgress from '../components/UploadProgress';

export default function DashboardPage() {
  const { fetchFiles, isLoading, error } = useFileStore();

  useEffect(() => {
    fetchFiles();
  }, [fetchFiles]);

  return (
    <div className="max-w-6xl mx-auto p-6">
      <h1 className="text-3xl font-bold text-gray-800 mb-8">My Files</h1>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-1">
          <FileUpload />
          <UploadProgress />
        </div>

        <div className="lg:col-span-2">
          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
              {error}
            </div>
          )}

          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
            </div>
          ) : (
            <FileList />
          )}
        </div>
      </div>
    </div>
  );
}
