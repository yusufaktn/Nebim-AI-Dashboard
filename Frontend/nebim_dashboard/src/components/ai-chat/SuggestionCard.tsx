import { cn } from '../../utils/formatters';
import type { ChatSuggestion } from '../../types';

interface SuggestionCardProps {
  suggestion: ChatSuggestion;
  onClick: (prompt: string) => void;
  className?: string;
}

export const SuggestionCard = ({ suggestion, onClick, className }: SuggestionCardProps) => {
  return (
    <button
      onClick={() => onClick(suggestion.prompt)}
      className={cn(
        'relative overflow-hidden bg-surface dark:bg-surface-dark rounded-xl sm:rounded-2xl p-3 sm:p-4 text-left',
        'ring-1 ring-black/5 dark:ring-white/10 hover:ring-primary/20 dark:hover:ring-primary/30',
        'shadow-sm hover:shadow-md dark:shadow-black/20',
        'transition-all duration-300 ease-out group',
        className
      )}
    >
      {/* Subtle gradient overlay on hover */}
      <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
      
      <div className="relative flex items-start gap-3">
        <div className="w-9 h-9 sm:w-10 sm:h-10 bg-gradient-to-br from-primary/10 to-primary/5 dark:from-primary/25 dark:to-primary/15 rounded-lg sm:rounded-xl flex items-center justify-center flex-shrink-0 group-hover:scale-105 transition-transform duration-300">
          <span className="material-symbols-outlined text-primary text-lg sm:text-xl">
            {suggestion.icon}
          </span>
        </div>
        <div className="flex-1 min-w-0">
          <p className="font-medium text-text-main dark:text-text-dark text-sm group-hover:text-primary transition-colors duration-200">
            {suggestion.title}
          </p>
          <p className="text-xs text-text-secondary dark:text-text-dark-secondary mt-1 sm:mt-1.5 line-clamp-2 leading-relaxed">
            {suggestion.description}
          </p>
        </div>
        <span className="hidden sm:inline material-symbols-outlined text-primary/50 text-lg translate-x-0 group-hover:translate-x-1 opacity-0 group-hover:opacity-100 transition-all duration-300">
          arrow_forward
        </span>
      </div>
    </button>
  );
};
