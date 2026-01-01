import { useMantineColorScheme } from '@mantine/core';
import { useCallback, useEffect } from 'react';

/**
 * Tema yönetimi için hook
 * Tek kaynak: Mantine color scheme (localStorage'a otomatik kaydedilir)
 * Tailwind dark mode class'ı da senkronize edilir
 */
export const useAppTheme = () => {
  const { colorScheme, toggleColorScheme, setColorScheme } = useMantineColorScheme();

  const isDark = colorScheme === 'dark';

  // Tailwind dark mode senkronizasyonu
  useEffect(() => {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [isDark]);

  const toggle = useCallback(() => {
    toggleColorScheme();
  }, [toggleColorScheme]);

  const setTheme = useCallback((theme: 'light' | 'dark') => {
    setColorScheme(theme);
  }, [setColorScheme]);

  return {
    colorScheme,
    isDark,
    toggle,
    setTheme,
  };
};

export default useAppTheme;
