import { create } from 'zustand';
import { persist } from 'zustand/middleware';

/**
 * UI State Store - Sadece sidebar durumu için
 * Tema yönetimi Mantine tarafından yapılır (AppProviders.tsx)
 */
interface UIState {
  sidebarOpen: boolean;
  setSidebarOpen: (open: boolean) => void;
  toggleSidebar: () => void;
}

export const useUIStore = create<UIState>()(
  persist(
    (set) => ({
      sidebarOpen: true,
      setSidebarOpen: (open) => set({ sidebarOpen: open }),
      toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
    }),
    {
      name: 'nebim-ui-storage',
    }
  )
);

// Geriye uyumluluk için alias (deprecate edilecek)
export const useThemeStore = useUIStore;
