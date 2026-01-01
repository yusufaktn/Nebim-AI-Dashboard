// API Yapılandırması
export const API_CONFIG = {
  BASE_URL: import.meta.env.VITE_API_URL || 'http://localhost:5026/api',
  TIMEOUT: 30000,
  RETRY_ATTEMPTS: 3,
} as const;

// JWT Yapılandırması
export const AUTH_CONFIG = {
  ACCESS_TOKEN_KEY: 'nebim_access_token',
  REFRESH_TOKEN_KEY: 'nebim_refresh_token',
  USER_KEY: 'nebim_user',
  TOKEN_REFRESH_THRESHOLD: 5 * 60 * 1000, // 5 dakika önce yenile
} as const;

// Uygulama Yapılandırması
export const APP_CONFIG = {
  NAME: 'Nebim Dashboard',
  VERSION: '1.0.0',
  DESCRIPTION: 'Kurumsal Yönetim Paneli',
  DEFAULT_LANGUAGE: 'tr',
  DATE_FORMAT: 'dd.MM.yyyy',
  DATETIME_FORMAT: 'dd.MM.yyyy HH:mm',
  CURRENCY: 'TRY',
  LOCALE: 'tr-TR',
} as const;

// Sayfalama Yapılandırması
export const PAGINATION_CONFIG = {
  DEFAULT_PAGE_SIZE: 10,
  PAGE_SIZE_OPTIONS: [10, 25, 50, 100],
} as const;

// Tema Yapılandırması
export const THEME_CONFIG = {
  STORAGE_KEY: 'nebim-theme-storage',
  DEFAULT_THEME: 'light' as const,
  THEMES: ['light', 'dark'] as const,
} as const;

// Bildirim Yapılandırması
export const NOTIFICATION_CONFIG = {
  POSITION: 'top-right' as const,
  AUTO_CLOSE: 5000,
  LIMIT: 5,
} as const;

// Dışa aktarma
export default {
  API_CONFIG,
  AUTH_CONFIG,
  APP_CONFIG,
  PAGINATION_CONFIG,
  THEME_CONFIG,
  NOTIFICATION_CONFIG,
};
