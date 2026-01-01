import apiClient from './apiClient';
import type { 
  ApiResponse, 
  DashboardResponse, 
  LowStockAlert 
} from '../types/api';

/**
 * Dashboard Service
 * Backend API ile dashboard verilerini çeker
 */
export const dashboardService = {
  /**
   * Dashboard özet verilerini getir
   */
  getDashboard: async (): Promise<DashboardResponse> => {
    const response = await apiClient.get<ApiResponse<DashboardResponse>>('/dashboard');
    return response.data.data;
  },

  /**
   * Düşük stok uyarılarını getir
   */
  getLowStockAlerts: async (threshold?: number): Promise<LowStockAlert[]> => {
    const params = threshold ? { threshold } : {};
    const response = await apiClient.get<ApiResponse<LowStockAlert[]>>(
      '/dashboard/low-stock-alerts',
      { params }
    );
    return response.data.data;
  },
};

export default dashboardService;
