import apiClient, { tokenStorage } from './apiClient';
import type {
  ApiResponse,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
  UserResponse,
} from '../types/api';

/**
 * Auth Service
 * Kimlik doğrulama işlemleri
 */
export const authService = {
  /**
   * Kullanıcı girişi
   */
  login: async (request: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>(
      '/auth/login',
      request
    );
    
    const { accessToken, refreshToken } = response.data.data;
    tokenStorage.setTokens(accessToken, refreshToken);
    
    return response.data.data;
  },

  /**
   * Yeni kullanıcı kaydı
   */
  register: async (request: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>(
      '/auth/register',
      request
    );
    
    const { accessToken, refreshToken } = response.data.data;
    tokenStorage.setTokens(accessToken, refreshToken);
    
    return response.data.data;
  },

  /**
   * Token yenileme
   */
  refreshToken: async (request: RefreshTokenRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>(
      '/auth/refresh',
      request
    );
    
    const { accessToken, refreshToken } = response.data.data;
    tokenStorage.setTokens(accessToken, refreshToken);
    
    return response.data.data;
  },

  /**
   * Çıkış yap
   */
  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout');
    } finally {
      tokenStorage.clearTokens();
    }
  },

  /**
   * Mevcut kullanıcı bilgisi
   */
  getCurrentUser: async (): Promise<UserResponse> => {
    const response = await apiClient.get<ApiResponse<UserResponse>>('/auth/me');
    return response.data.data;
  },

  /**
   * Token geçerli mi kontrol et
   */
  isAuthenticated: (): boolean => {
    return !!tokenStorage.getAccessToken();
  },
};

export default authService;
