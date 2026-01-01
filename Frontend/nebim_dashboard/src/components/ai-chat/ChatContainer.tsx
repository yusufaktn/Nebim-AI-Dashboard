import { useRef, useEffect } from 'react';
import { ChatMessage } from './ChatMessage';
import { SuggestionCard } from './SuggestionCard';
import { ChatInput } from './ChatInput';
import { useChatStore } from '../../store';
import { cn } from '../../utils/formatters';

interface ChatContainerProps {
  className?: string;
}

export const ChatContainer = ({ className }: ChatContainerProps) => {
  const { messages, isLoading, suggestions, sendMessage } = useChatStore();
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSuggestionClick = (prompt: string) => {
    sendMessage(prompt);
  };

  const hasMessages = messages.length > 0;

  return (
    <div className={cn('flex flex-col h-full', className)}>
      {/* Chat messages area */}
      <div className="flex-1 overflow-y-auto px-4 py-6">
        {!hasMessages ? (
          // Welcome screen
          <div className="flex flex-col items-center justify-center h-full max-w-2xl mx-auto px-4 sm:px-6">
            {/* AI Icon with soft glow */}
            <div className="relative mb-6 sm:mb-8">
              <div className="absolute inset-0 bg-primary/20 blur-2xl rounded-full scale-150" />
              <div className="relative w-20 h-20 sm:w-24 sm:h-24 bg-gradient-to-br from-primary/10 to-primary/5 dark:from-primary/20 dark:to-primary/10 rounded-[24px] sm:rounded-[28px] flex items-center justify-center shadow-lg shadow-primary/10">
                <span className="material-symbols-outlined text-primary text-4xl sm:text-5xl">
                  smart_toy
                </span>
              </div>
            </div>

            {/* Welcome text with better typography */}
            <h2 className="text-xl sm:text-2xl font-semibold text-text-main dark:text-text-dark text-center tracking-tight">
              Merhaba! Ben AI Asistanınız
            </h2>
            <p className="text-sm sm:text-base text-text-secondary dark:text-text-dark-secondary text-center mt-2 sm:mt-3 max-w-md leading-relaxed px-4">
              NebimFlow verilerinizi analiz etmenize, raporlar oluşturmanıza ve işletmeniz hakkında içgörüler edinmenize yardımcı olabilirim.
            </p>

            {/* Suggestion cards with responsive grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4 mt-8 sm:mt-10 w-full max-w-xl px-2 sm:px-0">
              {suggestions.map((suggestion) => (
                <SuggestionCard
                  key={suggestion.id}
                  suggestion={suggestion}
                  onClick={handleSuggestionClick}
                />
              ))}
            </div>
          </div>
        ) : (
          // Messages list
          <div className="space-y-6 max-w-4xl mx-auto">
            {messages.map((message) => (
              <ChatMessage key={message.id} message={message} />
            ))}

            {/* Loading indicator */}
            {isLoading && (
              <div className="flex justify-start animate-fade-in">
                <div className="flex gap-3">
                  <div className="w-9 h-9 rounded-full bg-gradient-to-br from-primary/15 to-primary/5 dark:from-primary/25 dark:to-primary/10 flex items-center justify-center shadow-sm">
                    <span className="material-symbols-outlined text-primary text-lg">
                      smart_toy
                    </span>
                  </div>
                  <div className="chat-bubble chat-bubble-ai shadow-sm">
                    <div className="flex items-center gap-3">
                      <div className="flex gap-1">
                        <span className="w-2 h-2 bg-primary/60 rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                        <span className="w-2 h-2 bg-primary/60 rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                        <span className="w-2 h-2 bg-primary/60 rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
                      </div>
                      <span className="text-sm text-text-secondary dark:text-text-dark-secondary">
                        Yanıt hazırlanıyor...
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            )}

            <div ref={messagesEndRef} />
          </div>
        )}
      </div>

      {/* Input area */}
      <div className="p-3 sm:p-4 bg-gradient-to-t from-background-light via-background-light to-transparent dark:from-background-dark dark:via-background-dark">
        <div className="max-w-4xl mx-auto">
          <ChatInput onSend={sendMessage} disabled={isLoading} />
        </div>
      </div>
    </div>
  );
};
