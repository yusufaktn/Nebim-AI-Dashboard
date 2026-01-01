// API Types (from backend)
export * from './api';

// Chat UI Types (Frontend specific)
export interface ChatSuggestion {
  id: string;
  icon: string;
  title: string;
  description: string;
  prompt: string;
}

// AISuggestion (Dashboard için)
export interface AISuggestion {
  id: string;
  title: string;
  description: string;
  type: 'info' | 'warning' | 'success';
  action?: string;
}

// Chat Message (UI için)
export interface ChatMessage {
  id: string | number;
  role: 'user' | 'assistant' | 'system';
  content: string;
  type?: 'text' | 'data' | 'chart' | 'error';
  timestamp?: Date;
  processingTimeMs?: number;
  data?: {
    stats?: Array<{ label: string; value: string | number; change?: number }>;
    [key: string]: unknown;
  };
  createdAt?: string;
}

// KPI Data (Dashboard için)
export interface KPIData {
  title: string;
  value: string | number;
  change?: number;
  changeType?: 'increase' | 'decrease';
  changeLabel?: string;
  icon?: string;
  color: 'primary' | 'success' | 'warning' | 'danger' | 'info';
}

// Sales Data (Chart için)
export interface SalesData {
  date: string;
  amount: number;
  count?: number;
  sales?: number;
  revenue?: number;
}

// Product (Stock için - eski bileşenler için)
export interface Product {
  id: string;
  productCode: string;
  productName: string;
  name?: string;
  code?: string;
  categoryName?: string;
  category?: string;
  brandName?: string;
  colorName?: string;
  sizeName?: string;
  quantity: number;
  stock?: number;
  minStock?: number;
  salePrice: number;
  price?: number;
  warehouseName?: string;
  image?: string;
  season?: string;
  status?: 'active' | 'inactive' | 'low-stock';
  salesSpeed?: number;
}

// Stock Filter
export interface StockFilter {
  searchTerm?: string;
  search?: string;
  category?: string;
  brand?: string;
  warehouse?: string;
  season?: string;
  status?: string;
  inStockOnly?: boolean;
  lowStockOnly?: boolean;
}

// Navigation Types
export interface NavItem {
  id: string;
  label: string;
  icon: string;
  path: string;
  badge?: number;
}

// Theme Types
export type Theme = 'light' | 'dark';
