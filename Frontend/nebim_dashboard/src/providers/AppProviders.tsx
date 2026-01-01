import type { ReactNode } from 'react';
import { MantineProvider } from '@mantine/core';
import { Notifications } from '@mantine/notifications';
import { theme, NOTIFICATION_CONFIG } from '../config';

interface AppProvidersProps {
  children: ReactNode;
}

/**
 * Uygulama genelinde kullanılan tüm provider'ları saran bileşen
 * - MantineProvider: UI tema ve stil sistemi (tema yönetimi burada)
 * - Notifications: Toast bildirimleri
 * 
 * Tema ayarı: defaultColorScheme="light", localStorage'a otomatik kaydedilir
 */
export const AppProviders = ({ children }: AppProvidersProps) => {
  return (
    <MantineProvider theme={theme} defaultColorScheme="light">
      <Notifications 
        position={NOTIFICATION_CONFIG.POSITION} 
        autoClose={NOTIFICATION_CONFIG.AUTO_CLOSE}
        limit={NOTIFICATION_CONFIG.LIMIT}
      />
      {children}
    </MantineProvider>
  );
};

export default AppProviders;
