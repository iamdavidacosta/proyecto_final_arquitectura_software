import { useFileStore } from '../store/fileStore';

export default function UploadProgress() {
  const uploadProgress = useFileStore((state) => state.uploadProgress);

  const activeUploads = uploadProgress.filter((p) => p.status === 'uploading');

  if (activeUploads.length === 0) {
    return null;
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-4 mt-4">
      <h3 className="text-sm font-medium text-gray-700 mb-3">Uploading...</h3>
      <div className="space-y-3">
        {activeUploads.map((upload) => (
          <div key={upload.fileId}>
            <div className="flex justify-between text-sm text-gray-600 mb-1">
              <span>Uploading file...</span>
              <span>{upload.progress}%</span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-2">
              <div
                className="bg-blue-500 h-2 rounded-full transition-all duration-300"
                style={{ width: `${upload.progress}%` }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
