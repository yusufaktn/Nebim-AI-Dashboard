import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserResponse } from '../types/api';
import { authService } from '../services/authService';
import { tokenStorage } from '../services/apiClient';

interface AuthState {
  user: UserResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  
  // Actions
  login: (email: string, password: string) => Promise<boolean>;
  register: (email: string, password: string, fullName: string) => Promise<boolean>;
  logout: () => Promise<void>;
  fetchCurrentUser: () => Promise<void>;
  clearError: () => void;
  checkAuth: () => Promise<boolean>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      login: async (email: string, password: string) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.login({ email, password });
          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
          });
          return true;
        } catch (error: unknown) {
          const message = error instanceof Error 
            ? error.message 
            : 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.';
          set({ error: message, isLoading: false });
          return false;
        }
      },

      register: async (email: string, password: string, fullName: string) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.register({ email, password, fullName });
          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
          });
          return true;
        } catch (error: unknown) {
          const message = error instanceof Error 
            ? error.message 
            : 'Kayıt başarısız. Lütfen tekrar deneyin.';
          set({ error: message, isLoading: false });
          return false;
        }
      },

      logout: async () => {
        set({ isLoading: true });
        try {
          await authService.logout();
        } finally {
          set({
            user: null,
            isAuthenticated: false,
            isLoading: false,
            error: null,
          });
        }
      },

      fetchCurrentUser: async () => {
        if (!tokenStorage.getAccessToken()) {
          set({ isAuthenticated: false, user: null });
          return;
        }
        
        set({ isLoading: true });
        try {
          const user = await authService.getCurrentUser();
          set({
            user,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch {
          set({
            user: null,
            isAuthenticated: false,
            isLoading: false,
          });
          tokenStorage.clearTokens();
        }
      },

      checkAuth: async () => {
        const { isAuthenticated, fetchCurrentUser } = get();
        
        if (!tokenStorage.getAccessToken()) {
          set({ isAuthenticated: false, user: null });
          return false;
        }
        
        if (!isAuthenticated) {
          await fetchCurrentUser();
        }
        
        return get().isAuthenticated;
      },

      clearError: () => set({ error: null }),
    }),
    {
      name: 'nebim-auth-store',
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

export default useAuthStore;
