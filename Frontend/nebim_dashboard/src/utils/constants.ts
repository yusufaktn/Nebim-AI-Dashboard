// API Configuration
export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// App Configuration
export const APP_NAME = 'Nebim Dashboard';
export const APP_VERSION = '1.0.0';

// Pagination
export const DEFAULT_PAGE_SIZE = 10;
export const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];

// Navigation Items
export const NAV_ITEMS = [
  { id: 'dashboard', label: 'Ana Sayfa', icon: 'home', path: '/' },
  { id: 'sales', label: 'Satış Raporu', icon: 'trending_up', path: '/sales' },
  { id: 'stock', label: 'Stok Listesi', icon: 'inventory_2', path: '/stock' },
  { id: 'ai', label: 'AI Asistan', icon: 'smart_toy', path: '/ai' },
  { id: 'settings', label: 'Ayarlar', icon: 'settings', path: '/settings' },
];

// Categories
export const PRODUCT_CATEGORIES = [
  { value: '', label: 'Tüm Kategoriler' },
  { value: 'ust-giyim', label: 'Üst Giyim' },
  { value: 'alt-giyim', label: 'Alt Giyim' },
  { value: 'dis-giyim', label: 'Dış Giyim' },
  { value: 'ayakkabi', label: 'Ayakkabı' },
  { value: 'aksesuar', label: 'Aksesuar' },
];

// Seasons
export const SEASONS = [
  { value: '', label: 'Tüm Sezonlar' },
  { value: '2025-kis', label: '2025 Kış' },
  { value: '2025-ilkbahar', label: '2025 İlkbahar' },
  { value: '2024-yaz', label: '2024 Yaz' },
  { value: '2024-sonbahar', label: '2024 Sonbahar' },
];

// Status Options
export const PRODUCT_STATUS = [
  { value: '', label: 'Tüm Durumlar' },
  { value: 'active', label: 'Aktif' },
  { value: 'low_stock', label: 'Düşük Stok' },
  { value: 'out_of_stock', label: 'Stok Yok' },
];

// Chart Colors
export const CHART_COLORS = {
  primary: '#2563EB',
  success: '#10B981',
  warning: '#F59E0B',
  danger: '#EF4444',
  info: '#3B82F6',
  purple: '#8B5CF6',
  pink: '#EC4899',
};

// Date Formats
export const DATE_FORMAT = 'DD.MM.YYYY';
export const TIME_FORMAT = 'HH:mm';
export const DATETIME_FORMAT = 'DD.MM.YYYY HH:mm';

// Currency
export const CURRENCY = {
  code: 'TRY',
  symbol: '₺',
  locale: 'tr-TR',
};
