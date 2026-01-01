import { notifications } from '@mantine/notifications';
import { IconCheck, IconX, IconInfoCircle, IconAlertTriangle } from '@tabler/icons-react';
import { createElement } from 'react';

type NotificationType = 'success' | 'error' | 'info' | 'warning';

interface NotifyOptions {
  title?: string;
  message: string;
  type?: NotificationType;
  autoClose?: number | false;
}

const iconMap = {
  success: IconCheck,
  error: IconX,
  info: IconInfoCircle,
  warning: IconAlertTriangle,
};

const colorMap = {
  success: 'green',
  error: 'red',
  info: 'blue',
  warning: 'orange',
};

/**
 * Bildirim gösterme hook'u
 */
export const useNotification = () => {
  const notify = ({ title, message, type = 'info', autoClose = 5000 }: NotifyOptions) => {
    const Icon = iconMap[type];
    
    notifications.show({
      title,
      message,
      color: colorMap[type],
      icon: createElement(Icon, { size: 18 }),
      autoClose,
    });
  };

  const success = (message: string, title?: string) => {
    notify({ message, title: title || 'Başarılı', type: 'success' });
  };

  const error = (message: string, title?: string) => {
    notify({ message, title: title || 'Hata', type: 'error' });
  };

  const info = (message: string, title?: string) => {
    notify({ message, title: title || 'Bilgi', type: 'info' });
  };

  const warning = (message: string, title?: string) => {
    notify({ message, title: title || 'Uyarı', type: 'warning' });
  };

  return {
    notify,
    success,
    error,
    info,
    warning,
  };
};

export default useNotification;
