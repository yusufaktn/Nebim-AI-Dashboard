import { useState, useRef, useEffect } from 'react';
import { cn } from '../../utils/formatters';

interface ChatInputProps {
  onSend: (message: string) => void;
  disabled?: boolean;
  className?: string;
}

export const ChatInput = ({ onSend, disabled, className }: ChatInputProps) => {
  const [message, setMessage] = useState('');
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Auto-resize textarea
  useEffect(() => {
    const textarea = textareaRef.current;
    if (textarea) {
      textarea.style.height = 'auto';
      textarea.style.height = Math.min(textarea.scrollHeight, 200) + 'px';
    }
  }, [message]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (message.trim() && !disabled) {
      onSend(message.trim());
      setMessage('');
      if (textareaRef.current) {
        textareaRef.current.style.height = 'auto';
      }
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  return (
    <form onSubmit={handleSubmit} className={cn('relative', className)}>
      <div className="flex items-end gap-2 sm:gap-3 p-2 sm:p-3 bg-surface dark:bg-surface-dark rounded-xl sm:rounded-2xl shadow-lg shadow-black/5 dark:shadow-black/30 ring-1 ring-black/5 dark:ring-white/10 focus-within:ring-2 focus-within:ring-primary/30 transition-all duration-200">
        {/* Attachment button - Hidden on very small screens */}
        <button
          type="button"
          className="hidden sm:flex p-2 rounded-xl hover:bg-black/5 dark:hover:bg-white/10 transition-colors duration-200 flex-shrink-0"
        >
          <span className="material-symbols-outlined text-text-secondary dark:text-text-dark-secondary text-xl">
            attach_file
          </span>
        </button>

        {/* Textarea */}
        <textarea
          ref={textareaRef}
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Sorunuzu yazın..."
          disabled={disabled}
          rows={1}
          className={cn(
            'flex-1 resize-none bg-transparent border-none outline-none text-sm text-text-main dark:text-text-dark placeholder:text-text-muted/70 dark:placeholder:text-text-dark-secondary/50',
            'max-h-[200px] min-h-[24px] py-2 px-1 sm:px-0',
            disabled && 'opacity-50 cursor-not-allowed'
          )}
        />

        {/* Send button */}
        <button
          type="submit"
          disabled={disabled || !message.trim()}
          className={cn(
            'p-2 sm:p-2.5 rounded-lg sm:rounded-xl transition-all duration-200 flex-shrink-0',
            message.trim() && !disabled
              ? 'bg-primary text-white hover:bg-primary-hover shadow-md shadow-primary/25 hover:shadow-lg hover:shadow-primary/30 active:scale-95'
              : 'bg-black/5 dark:bg-white/10 text-text-muted dark:text-text-dark-secondary cursor-not-allowed'
          )}
        >
          {disabled ? (
            <div className="flex gap-0.5">
              <span className="w-1.5 h-1.5 bg-white rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
              <span className="w-1.5 h-1.5 bg-white rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
              <span className="w-1.5 h-1.5 bg-white rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
            </div>
          ) : (
            <span className="material-symbols-outlined text-lg sm:text-xl">send</span>
          )}
        </button>
      </div>

      {/* Helper text - Hidden on mobile */}
      <p className="hidden sm:block text-[11px] text-text-muted/60 dark:text-text-dark-secondary/50 text-center mt-2">
        Shift + Enter ile yeni satır
      </p>
    </form>
  );
};
