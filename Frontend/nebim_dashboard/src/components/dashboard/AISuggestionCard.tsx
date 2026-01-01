import { cn } from '../../utils/formatters';
import type { AISuggestion } from '../../types';

interface AISuggestionCardProps {
  suggestions: AISuggestion[];
  className?: string;
}

const iconStyles: Record<AISuggestion['type'], { bg: string; icon: string; iconName: string }> = {
  warning: {
    bg: 'bg-warning-light dark:bg-warning/20',
    icon: 'text-warning-dark dark:text-warning',
    iconName: 'warning',
  },
  info: {
    bg: 'bg-info-light dark:bg-info/20',
    icon: 'text-info-dark dark:text-info',
    iconName: 'info',
  },
  success: {
    bg: 'bg-success-light dark:bg-success/20',
    icon: 'text-success-dark dark:text-success',
    iconName: 'check_circle',
  },
};

export const AISuggestionCard = ({ suggestions, className }: AISuggestionCardProps) => {
  return (
    <div className={cn('card', className)}>
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        <div className="w-10 h-10 bg-primary/10 dark:bg-primary/20 rounded-xl flex items-center justify-center">
          <span className="material-symbols-outlined text-primary text-xl">
            smart_toy
          </span>
        </div>
        <div>
          <h3 className="text-lg font-semibold text-text-main dark:text-text-dark">
            AI Önerileri
          </h3>
          <p className="text-sm text-text-secondary dark:text-text-dark-secondary">
            Yapay zeka destekli içgörüler
          </p>
        </div>
      </div>

      {/* Suggestions list */}
      <div className="space-y-4">
        {suggestions.map((suggestion) => {
          const style = iconStyles[suggestion.type];
          return (
            <div
              key={suggestion.id}
              className="flex items-start gap-3 p-3 rounded-lg bg-background-light dark:bg-background-dark hover:bg-surface-hover dark:hover:bg-surface-dark transition-colors cursor-pointer"
            >
              <div className={cn('w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0', style.bg)}>
                <span className={cn('material-symbols-outlined text-lg', style.icon)}>
                  {style.iconName}
                </span>
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-medium text-text-main dark:text-text-dark text-sm">
                  {suggestion.title}
                </p>
                <p className="text-sm text-text-secondary dark:text-text-dark-secondary mt-0.5 line-clamp-2">
                  {suggestion.description}
                </p>
                {suggestion.action && (
                  <button className="text-primary text-sm font-medium mt-2 hover:underline">
                    {suggestion.action} →
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* View all button */}
      <button className="w-full mt-4 py-3 text-center text-primary font-medium hover:bg-primary/5 rounded-lg transition-colors">
        Tüm Önerileri Gör
      </button>
    </div>
  );
};
