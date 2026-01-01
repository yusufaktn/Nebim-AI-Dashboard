import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { chatService } from '../services';
import type { 
  ChatSessionResponse, 
  ChatMessageResponse 
} from '../types/api';
import type { ChatMessage, ChatSuggestion } from '../types';

interface ChatState {
  // State
  currentSession: ChatSessionResponse | null;
  messages: ChatMessage[];
  sessions: ChatSessionResponse[];
  isLoading: boolean;
  error: string | null;
  suggestions: ChatSuggestion[];
  
  // Actions
  startSession: (title?: string) => Promise<void>;
  sendMessage: (content: string) => Promise<void>;
  loadSession: (sessionId: number) => Promise<void>;
  loadSessions: () => Promise<void>;
  deleteSession: (sessionId: number) => Promise<void>;
  clearMessages: () => void;
  clearError: () => void;
  setLoading: (loading: boolean) => void;
  addMessage: (message: Omit<ChatMessage, 'id' | 'timestamp'>) => void;
}

// Default suggestions for the AI assistant
const defaultSuggestions: ChatSuggestion[] = [
  {
    id: '1',
    icon: 'trending_up',
    title: 'Satış Analizi',
    description: 'Bu haftanın satış performansını analiz et',
    prompt: 'Bu haftanın satış performansını analiz edebilir misin?',
  },
  {
    id: '2',
    icon: 'inventory_2',
    title: 'Stok Durumu',
    description: 'Kritik stok seviyesindeki ürünleri listele',
    prompt: 'Kritik stok seviyesindeki ürünleri listele',
  },
  {
    id: '3',
    icon: 'insights',
    title: 'Trend Analizi',
    description: 'En çok satan ürün kategorilerini göster',
    prompt: 'En çok satan ürün kategorileri hangileri?',
  },
  {
    id: '4',
    icon: 'analytics',
    title: 'Günlük Özet',
    description: 'Bugünkü satış ve stok durumunu özetle',
    prompt: 'Bugünkü satış ve stok durumunu özetler misin?',
  },
];

// Backend enum değerlerini string'e çevir
const roleMap: Record<number | string, 'user' | 'assistant' | 'system'> = {
  0: 'system',
  1: 'assistant',
  2: 'user',
  'System': 'system',
  'Assistant': 'assistant', 
  'User': 'user',
  'system': 'system',
  'assistant': 'assistant',
  'user': 'user',
};

// API response'u frontend ChatMessage'a dönüştür
const mapApiMessageToLocal = (msg: ChatMessageResponse): ChatMessage => {
  const role = roleMap[msg.role as keyof typeof roleMap] || 'assistant';
  return {
    id: msg.id,
    role,
    content: msg.content,
    timestamp: new Date(msg.createdAt),
    data: msg.data as ChatMessage['data'],
    processingTimeMs: msg.processingTimeMs,
  };
};

export const useChatStore = create<ChatState>()(
  persist(
    (set, get) => ({
      currentSession: null,
      messages: [],
      sessions: [],
      isLoading: false,
      error: null,
      suggestions: defaultSuggestions,

      startSession: async (title?: string) => {
        set({ isLoading: true, error: null });
        try {
          const session = await chatService.startSession({ title });
          set({ 
            currentSession: session, 
            messages: [],
            isLoading: false 
          });
          // Oturum listesini güncelle
          get().loadSessions();
        } catch (error) {
          const message = error instanceof Error ? error.message : 'Oturum başlatılamadı';
          set({ error: message, isLoading: false });
        }
      },

      sendMessage: async (content: string) => {
        const { currentSession } = get();
        
        // Oturum yoksa yeni başlat
        if (!currentSession) {
          await get().startSession();
        }
        
        const session = get().currentSession;
        if (!session) {
          set({ error: 'Oturum başlatılamadı' });
          return;
        }

        // Kullanıcı mesajını ekle (optimistic update)
        const userMessage: ChatMessage = {
          id: `temp-${Date.now()}`,
          role: 'user',
          content,
          timestamp: new Date(),
        };
        
        set((state) => ({ 
          messages: [...state.messages, userMessage],
          isLoading: true,
          error: null 
        }));

        try {
          // API'ye mesaj gönder
          const response = await chatService.sendMessage(session.id, { message: content });
          
          // AI yanıtını ekle
          const aiMessage = mapApiMessageToLocal(response);
          
          set((state) => ({
            messages: [...state.messages, aiMessage],
            isLoading: false,
          }));
        } catch (error) {
          const message = error instanceof Error ? error.message : 'Mesaj gönderilemedi';
          set({ error: message, isLoading: false });
        }
      },

      loadSession: async (sessionId: number) => {
        set({ isLoading: true, error: null });
        try {
          const session = await chatService.getSession(sessionId);
          const messages = session.messages?.map(mapApiMessageToLocal) || [];
          
          set({ 
            currentSession: session, 
            messages,
            isLoading: false 
          });
        } catch (error) {
          const message = error instanceof Error ? error.message : 'Oturum yüklenemedi';
          set({ error: message, isLoading: false });
        }
      },

      loadSessions: async () => {
        try {
          const sessions = await chatService.getMySessions();
          set({ sessions });
        } catch (error) {
          console.error('Oturumlar yüklenirken hata:', error);
        }
      },

      deleteSession: async (sessionId: number) => {
        try {
          await chatService.deleteSession(sessionId);
          
          // Mevcut oturum siliniyorsa temizle
          const { currentSession } = get();
          if (currentSession?.id === sessionId) {
            set({ currentSession: null, messages: [] });
          }
          
          // Listeyi güncelle
          get().loadSessions();
        } catch (error) {
          const message = error instanceof Error ? error.message : 'Oturum silinemedi';
          set({ error: message });
        }
      },

      clearMessages: () => set({ messages: [], currentSession: null }),

      clearError: () => set({ error: null }),

      setLoading: (loading: boolean) => set({ isLoading: loading }),

      addMessage: (message) => {
        const newMessage: ChatMessage = {
          ...message,
          id: `local-${Date.now()}`,
          timestamp: new Date(),
        };
        set((state) => ({ messages: [...state.messages, newMessage] }));
      },
    }),
    {
      name: 'nebim-chat-store',
      partialize: (state) => ({
        sessions: state.sessions,
      }),
    }
  )
);

export default useChatStore;
