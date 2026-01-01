import apiClient from './apiClient';
import type {
  ApiResponse,
  PagedApiResponse,
  NebimStockDto,
  NebimProductDto,
  StockFilterRequest,
} from '../types/api';

/**
 * Stock Service
 * Backend API ile stok verilerini çeker
 */
export const stockService = {
  /**
   * Stok listesi (sayfalı)
   */
  getStocks: async (filter: StockFilterRequest = {}): Promise<PagedApiResponse<NebimStockDto>> => {
    const response = await apiClient.get<PagedApiResponse<NebimStockDto>>('/stock', {
      params: filter,
    });
    return response.data;
  },

  /**
   * Ürün detayı
   */
  getProduct: async (productCode: string): Promise<NebimProductDto | null> => {
    try {
      const response = await apiClient.get<ApiResponse<NebimProductDto>>(
        `/stock/products/${productCode}`
      );
      return response.data.data;
    } catch {
      return null;
    }
  },

  /**
   * Ürün ara
   */
  searchProducts: async (term: string, limit: number = 20): Promise<NebimProductDto[]> => {
    const response = await apiClient.get<ApiResponse<NebimProductDto[]>>(
      '/stock/products/search',
      { params: { term, limit } }
    );
    return response.data.data;
  },

  /**
   * Kategori listesi
   */
  getCategories: async (): Promise<string[]> => {
    const response = await apiClient.get<ApiResponse<string[]>>('/stock/categories');
    return response.data.data;
  },

  /**
   * Marka listesi
   */
  getBrands: async (): Promise<string[]> => {
    const response = await apiClient.get<ApiResponse<string[]>>('/stock/brands');
    return response.data.data;
  },
};

export default stockService;
