import apiClient from './apiClient';
import type {
  ApiResponse,
  ChatSessionResponse,
  ChatMessageResponse,
  SendMessageRequest,
  StartSessionRequest,
} from '../types/api';

/**
 * Chat Service
 * AI asistan ile sohbet işlemleri
 */
export const chatService = {
  /**
   * Yeni chat oturumu başlat
   */
  startSession: async (request?: StartSessionRequest): Promise<ChatSessionResponse> => {
    const response = await apiClient.post<ApiResponse<ChatSessionResponse>>(
      '/chat/sessions',
      request || {}
    );
    return response.data.data;
  },

  /**
   * Mesaj gönder
   */
  sendMessage: async (
    sessionId: number,
    request: SendMessageRequest
  ): Promise<ChatMessageResponse> => {
    const response = await apiClient.post<ApiResponse<ChatMessageResponse>>(
      `/chat/sessions/${sessionId}/messages`,
      request
    );
    return response.data.data;
  },

  /**
   * Oturum detayı (mesajlarla birlikte)
   */
  getSession: async (sessionId: number): Promise<ChatSessionResponse> => {
    const response = await apiClient.get<ApiResponse<ChatSessionResponse>>(
      `/chat/sessions/${sessionId}`
    );
    return response.data.data;
  },

  /**
   * Kullanıcının tüm oturumları
   */
  getMySessions: async (): Promise<ChatSessionResponse[]> => {
    const response = await apiClient.get<ApiResponse<ChatSessionResponse[]>>(
      '/chat/sessions'
    );
    return response.data.data;
  },

  /**
   * Oturum sil
   */
  deleteSession: async (sessionId: number): Promise<void> => {
    await apiClient.delete(`/chat/sessions/${sessionId}`);
  },
};

export default chatService;
