import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import axios from 'axios';

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

interface AuthState {
  token: string | null;
  refreshToken: string | null;
  user: User | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, confirmPassword: string, firstName: string, lastName: string) => Promise<void>;
  logout: () => void;
  refreshAccessToken: () => Promise<void>;
}

const API_URL = '/api/auth';

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,

      login: async (email: string, password: string) => {
        const response = await axios.post(`${API_URL}/login`, { email, password });
        const { accessToken, refreshToken, userId, email: userEmail, firstName, lastName } = response.data;
        
        set({
          token: accessToken,
          refreshToken,
          user: { id: userId, email: userEmail, firstName, lastName },
          isAuthenticated: true,
        });

        axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      },

      register: async (email: string, password: string, confirmPassword: string, firstName: string, lastName: string) => {
        const response = await axios.post(`${API_URL}/register`, {
          email,
          password,
          confirmPassword,
          firstName,
          lastName,
        });
        const { accessToken, refreshToken, userId, email: userEmail } = response.data;
        
        set({
          token: accessToken,
          refreshToken,
          user: { id: userId, email: userEmail, firstName, lastName },
          isAuthenticated: true,
        });

        axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      },

      logout: () => {
        set({
          token: null,
          refreshToken: null,
          user: null,
          isAuthenticated: false,
        });
        delete axios.defaults.headers.common['Authorization'];
      },

      refreshAccessToken: async () => {
        const { refreshToken } = get();
        if (!refreshToken) throw new Error('No refresh token');

        const response = await axios.post(`${API_URL}/refresh`, { refreshToken });
        const { accessToken, refreshToken: newRefreshToken } = response.data;
        
        set({
          token: accessToken,
          refreshToken: newRefreshToken,
        });

        axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        token: state.token,
        refreshToken: state.refreshToken,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

// Set up axios interceptor for token refresh
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        await useAuthStore.getState().refreshAccessToken();
        return axios(originalRequest);
      } catch {
        useAuthStore.getState().logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

// Initialize axios with stored token
const storedToken = useAuthStore.getState().token;
if (storedToken) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`;
}
