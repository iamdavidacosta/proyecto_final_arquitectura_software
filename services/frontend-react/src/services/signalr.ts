import * as signalR from '@microsoft/signalr';

const SIGNALR_URL = import.meta.env.VITE_SIGNALR_URL || 'http://localhost:5000/hubs/file-upload';

export interface UploadProgress {
  fileId: string;
  fileName: string;
  progress: number;
  status: 'uploading' | 'processing' | 'completed' | 'failed';
  message?: string;
}

type ProgressCallback = (progress: UploadProgress) => void;
type ConnectionStateCallback = (state: signalR.HubConnectionState) => void;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private progressCallbacks: Set<ProgressCallback> = new Set();
  private connectionStateCallbacks: Set<ConnectionStateCallback> = new Set();
  private _reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  private get reconnectAttempts(): number {
    return this._reconnectAttempts;
  }

  private set reconnectAttempts(value: number) {
    this._reconnectAttempts = value;
  }

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = localStorage.getItem('accessToken');
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_URL, {
        accessTokenFactory: () => token || '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext: signalR.RetryContext) => {
          if (retryContext.previousRetryCount >= this.maxReconnectAttempts) {
            return null; // Stop reconnecting
          }
          // Exponential backoff: 0, 2, 4, 8, 16 seconds
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    this.connection.on('UploadProgress', (progress: UploadProgress) => {
      this.notifyProgressCallbacks(progress);
    });

    this.connection.on('UploadComplete', (progress: UploadProgress) => {
      this.notifyProgressCallbacks({ ...progress, status: 'completed', progress: 100 });
    });

    this.connection.on('UploadFailed', (progress: UploadProgress) => {
      this.notifyProgressCallbacks({ ...progress, status: 'failed' });
    });

    this.connection.on('ProcessingStarted', (progress: UploadProgress) => {
      this.notifyProgressCallbacks({ ...progress, status: 'processing' });
    });

    // Connection state change handlers
    this.connection.onreconnecting(() => {
      console.log('SignalR: Reconnecting...');
      this.notifyConnectionStateCallbacks(signalR.HubConnectionState.Reconnecting);
    });

    this.connection.onreconnected(() => {
      console.log('SignalR: Reconnected');
      this.reconnectAttempts = 0;
      this.notifyConnectionStateCallbacks(signalR.HubConnectionState.Connected);
    });

    this.connection.onclose((error: Error | undefined) => {
      console.log('SignalR: Connection closed', error);
      this.notifyConnectionStateCallbacks(signalR.HubConnectionState.Disconnected);
    });

    try {
      await this.connection.start();
      console.log('SignalR: Connected');
      this.notifyConnectionStateCallbacks(signalR.HubConnectionState.Connected);
    } catch (error) {
      console.error('SignalR: Connection failed', error);
      this.notifyConnectionStateCallbacks(signalR.HubConnectionState.Disconnected);
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  async subscribeToFile(fileId: string): Promise<void> {
    if (this.connection?.state !== signalR.HubConnectionState.Connected) {
      await this.connect();
    }
    await this.connection?.invoke('SubscribeToFile', fileId);
  }

  async unsubscribeFromFile(fileId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('UnsubscribeFromFile', fileId);
    }
  }

  onProgress(callback: ProgressCallback): () => void {
    this.progressCallbacks.add(callback);
    return () => this.progressCallbacks.delete(callback);
  }

  onConnectionStateChange(callback: ConnectionStateCallback): () => void {
    this.connectionStateCallbacks.add(callback);
    return () => this.connectionStateCallbacks.delete(callback);
  }

  private notifyProgressCallbacks(progress: UploadProgress): void {
    this.progressCallbacks.forEach((callback) => callback(progress));
  }

  private notifyConnectionStateCallbacks(state: signalR.HubConnectionState): void {
    this.connectionStateCallbacks.forEach((callback) => callback(state));
  }

  getConnectionState(): signalR.HubConnectionState {
    return this.connection?.state ?? signalR.HubConnectionState.Disconnected;
  }
}

// Singleton instance
export const signalRService = new SignalRService();
export default signalRService;
