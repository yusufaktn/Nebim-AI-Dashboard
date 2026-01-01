// ============================================
// API Response Types (Backend'den dönen formatlar)
// ============================================

/** Standart API Response wrapper */
export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  timestamp: string;
}

/** Hata Response */
export interface ApiErrorResponse {
  success: false;
  message: string;
  errors?: string[];
  traceId?: string;
  timestamp: string;
}

/** Sayfalı Response */
export interface PagedApiResponse<T> {
  success: boolean;
  items: T[];
  pagination: PaginationMeta;
}

export interface PaginationMeta {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// ============================================
// Auth Types
// ============================================

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserResponse;
}

export interface UserResponse {
  id: number;
  email: string;
  fullName: string;
  role: 'Admin' | 'User';
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
}

// ============================================
// Dashboard Types (Backend DashboardResponse ile uyumlu)
// ============================================

/** Dashboard ana response - Backend ile senkron */
export interface DashboardResponse {
  dailySales: DailySalesSummary;
  stockSummary: StockSummary;
  topProducts: TopProductDto[];
  storeSales: StoreSalesDto[];
  salesTrend: DailySalesTrendDto[];
}

/** Günlük satış özeti */
export interface DailySalesSummary {
  todayTotal: number;
  todayCount: number;
  yesterdayTotal: number;
  changePercentage: number;
  monthlyTotal: number;
}

/** Stok özeti */
export interface StockSummary {
  totalProducts: number;
  totalQuantity: number;
  totalValue: number;
  lowStockCount: number;
  outOfStockCount: number;
}

/** En çok satan ürün */
export interface TopProductDto {
  productCode: string;
  productName: string;
  totalQuantity: number;
  totalAmount: number;
}

/** Mağaza bazlı satış */
export interface StoreSalesDto {
  storeCode: string;
  storeName: string;
  totalAmount: number;
  salesCount: number;
}

/** Günlük satış trendi */
export interface DailySalesTrendDto {
  date: string;
  amount: number;
  salesCount: number;
}

/** Düşük stok uyarısı - Backend LowStockAlertDto ile uyumlu */
export interface LowStockAlert {
  productCode: string;
  productName: string;
  warehouseName: string;
  currentQuantity: number;
  severity: 'Critical' | 'Warning';
}

// ============================================
// Stock Types (Backend NebimStockDto ile uyumlu)
// ============================================

/** Nebim stok DTO - Backend ile senkron */
export interface NebimStockDto {
  productCode: string;
  productName: string;
  colorName?: string;
  sizeName?: string;
  warehouseCode: string;
  warehouseName?: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  unitCode: string;
  lastUpdatedAt?: string;
}

/** Nebim ürün DTO - Backend ile senkron */
export interface NebimProductDto {
  productCode: string;
  productName: string;
  categoryName?: string;
  subCategoryName?: string;
  brandName?: string;
  colorName?: string;
  sizeName?: string;
  barcode?: string;
  salePrice: number;
  costPrice?: number;
  currencyCode: string;
  vatRate: number;
  isActive: boolean;
}

/** Stok filtreleme - Backend StockFilterRequest ile uyumlu */
export interface StockFilterRequest {
  page?: number;
  pageSize?: number;
  productCode?: string;
  productName?: string;
  categoryName?: string;
  brandName?: string;
  warehouseCode?: string;
  minQuantity?: number;
  maxQuantity?: number;
  inStockOnly?: boolean;
  lowStockOnly?: boolean;
}

// ============================================
// Chat Types (Backend ChatResponse ile uyumlu)
// ============================================

export interface ChatSessionResponse {
  id: number;
  title: string;
  isActive: boolean;
  lastMessageAt?: string;
  messageCount: number;
  createdAt: string;
  messages?: ChatMessageResponse[];
}

export interface ChatMessageResponse {
  id: number;
  role: 'User' | 'Assistant' | 'System' | number;  // Backend hem string hem enum gönderebilir
  content: string;
  type: 'Text' | 'Data' | 'Chart' | 'Error' | number;
  data?: unknown;
  processingTimeMs?: number;
  createdAt: string;
}

export interface SendMessageRequest {
  message: string;
}

export interface StartSessionRequest {
  title?: string;
}

/** Chat oturum özeti - Liste için */
export interface ChatSessionSummary {
  id: number;
  title: string;
  isActive: boolean;
  lastMessageAt?: string;
  messageCount: number;
  createdAt: string;
}

// ============================================
// Sales Types (Satış raporu için)
// ============================================

/** Nebim satış DTO */
export interface NebimSaleDto {
  receiptNumber: string;
  saleDate: string;
  storeCode: string;
  storeName?: string;
  productCode: string;
  productName: string;
  colorName?: string;
  sizeName?: string;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  totalAmount: number;
  vatAmount: number;
  currencyCode: string;
  customerCode?: string;
  saleType: string;
  paymentMethod?: string;
}

/** Satış filtreleme */
export interface SalesFilterRequest {
  page?: number;
  pageSize?: number;
  startDate?: string;
  endDate?: string;
  storeCode?: string;
  productCode?: string;
  customerCode?: string;
  minAmount?: number;
  maxAmount?: number;
}
