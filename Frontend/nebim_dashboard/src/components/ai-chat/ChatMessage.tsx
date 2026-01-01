import { cn } from '../../utils/formatters';
import type { ChatMessage as ChatMessageType } from '../../types';

interface ChatMessageProps {
  message: ChatMessageType;
  className?: string;
}

export const ChatMessage = ({ message, className }: ChatMessageProps) => {
  const isUser = message.role === 'user';

  return (
    <div
      className={cn(
        'flex w-full animate-fade-in',
        isUser ? 'justify-end' : 'justify-start',
        className
      )}
    >
      <div className={cn('flex gap-3 max-w-[80%]', isUser && 'flex-row-reverse')}>
        {/* Avatar */}
        <div
          className={cn(
            'w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0 shadow-sm',
            isUser 
              ? 'bg-gradient-to-br from-primary to-primary-dark' 
              : 'bg-gradient-to-br from-primary/15 to-primary/5 dark:from-primary/25 dark:to-primary/10'
          )}
        >
          {isUser ? (
            <span className="text-white text-sm font-medium">U</span>
          ) : (
            <span className="material-symbols-outlined text-primary text-lg">
              smart_toy
            </span>
          )}
        </div>

        {/* Message bubble */}
        <div
          className={cn(
            'chat-bubble shadow-sm',
            isUser ? 'chat-bubble-user' : 'chat-bubble-ai'
          )}
        >
          {/* Message content */}
          <div className="text-sm whitespace-pre-wrap">{message.content}</div>

          {/* Stats data if present */}
          {message.data?.type === 'stats' && message.data.stats && (
            <div className="mt-4 grid grid-cols-1 sm:grid-cols-3 gap-3">
              {message.data.stats.map((stat, index) => (
                <div
                  key={index}
                  className="bg-background-light dark:bg-background-dark rounded-lg p-3"
                >
                  <p className="text-xs text-text-secondary dark:text-text-dark-secondary">
                    {stat.label}
                  </p>
                  <p className="text-lg font-bold text-text-main dark:text-text-dark mt-1">
                    {stat.value}
                  </p>
                  {stat.change !== undefined && (
                    <p
                      className={cn(
                        'text-xs font-medium mt-1',
                        stat.change >= 0 ? 'text-success' : 'text-danger'
                      )}
                    >
                      {stat.change >= 0 ? '+' : ''}{stat.change}%
                    </p>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
