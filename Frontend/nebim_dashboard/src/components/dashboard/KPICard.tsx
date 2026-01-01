import { cn } from '../../utils/formatters';
import type { KPIData } from '../../types';

interface KPICardProps {
  data: KPIData;
  className?: string;
}

const colorClasses: Record<KPIData['color'], { bg: string; icon: string }> = {
  primary: { bg: 'bg-blue-100 dark:bg-blue-900/30', icon: 'text-blue-600 dark:text-blue-400' },
  success: { bg: 'bg-green-100 dark:bg-green-900/30', icon: 'text-green-600 dark:text-green-400' },
  info: { bg: 'bg-purple-100 dark:bg-purple-900/30', icon: 'text-purple-600 dark:text-purple-400' },
  warning: { bg: 'bg-orange-100 dark:bg-orange-900/30', icon: 'text-orange-600 dark:text-orange-400' },
  danger: { bg: 'bg-red-100 dark:bg-red-900/30', icon: 'text-red-600 dark:text-red-400' },
};

export const KPICard = ({ data, className }: KPICardProps) => {
  const colors = colorClasses[data.color];
  const isPositive = data.changeType === 'increase';

  return (
    <div className={cn('card', className)}>
      <div className="flex items-start justify-between">
        {/* Icon */}
        <div className={cn('w-12 h-12 rounded-xl flex items-center justify-center', colors.bg)}>
          <span className={cn('material-symbols-outlined text-2xl', colors.icon)}>
            {data.icon}
          </span>
        </div>

        {/* Change indicator */}
        <div
          className={cn(
            'flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium',
            isPositive
              ? 'bg-success-light text-success-dark'
              : 'bg-danger-light text-danger-dark'
          )}
        >
          <span className="material-symbols-outlined text-sm">
            {isPositive ? 'trending_up' : 'trending_down'}
          </span>
          <span>{Math.abs(data.change ?? 0)}%</span>
        </div>
      </div>

      {/* Value */}
      <div className="mt-4">
        <p className="text-2xl font-bold text-text-main dark:text-text-dark">
          {data.value}
        </p>
        <p className="text-sm text-text-secondary dark:text-text-dark-secondary mt-1">
          {data.title}
        </p>
      </div>
    </div>
  );
};
