import apiClient from './apiClient';
import type {
  ApiResponse,
  PagedApiResponse,
  NebimSaleDto,
  SalesFilterRequest,
} from '../types/api';

/** Mağaza bilgisi */
export interface StoreInfo {
  code: string;
  name: string;
}

/** Günlük satış verisi */
export interface DailySalesData {
  [date: string]: number;
}

/**
 * Sales Service
 * Backend API ile satış verilerini çeker
 */
export const salesService = {
  /**
   * Satış listesi (sayfalı)
   */
  getSales: async (filter: SalesFilterRequest = {}): Promise<PagedApiResponse<NebimSaleDto>> => {
    const response = await apiClient.get<PagedApiResponse<NebimSaleDto>>('/sales', {
      params: filter,
    });
    return response.data;
  },

  /**
   * En çok satan ürünler
   */
  getTopSellingProducts: async (
    startDate?: string,
    endDate?: string,
    limit: number = 10
  ): Promise<NebimSaleDto[]> => {
    const response = await apiClient.get<ApiResponse<NebimSaleDto[]>>('/sales/top-products', {
      params: { startDate, endDate, limit },
    });
    return response.data.data;
  },

  /**
   * Günlük satış özeti
   */
  getDailySales: async (
    startDate?: string,
    endDate?: string
  ): Promise<DailySalesData> => {
    const response = await apiClient.get<ApiResponse<DailySalesData>>('/sales/daily', {
      params: { startDate, endDate },
    });
    return response.data.data;
  },

  /**
   * Toplam satış tutarı
   */
  getTotalSalesAmount: async (
    startDate?: string,
    endDate?: string
  ): Promise<number> => {
    const response = await apiClient.get<ApiResponse<number>>('/sales/total', {
      params: { startDate, endDate },
    });
    return response.data.data;
  },

  /**
   * Mağaza listesi
   */
  getStores: async (): Promise<StoreInfo[]> => {
    const response = await apiClient.get<ApiResponse<StoreInfo[]>>('/sales/stores');
    return response.data.data;
  },
};

export default salesService;
