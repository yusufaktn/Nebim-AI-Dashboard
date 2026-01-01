import { cn } from '../../utils/formatters';
import { PRODUCT_CATEGORIES, SEASONS, PRODUCT_STATUS } from '../../utils/constants';
import type { StockFilter } from '../../types';

interface StockFiltersProps {
  filter: StockFilter;
  onFilterChange: (filter: StockFilter) => void;
  className?: string;
}

export const StockFilters = ({ filter, onFilterChange, className }: StockFiltersProps) => {
  const handleChange = (key: keyof StockFilter, value: string) => {
    onFilterChange({ ...filter, [key]: value });
  };

  return (
    <div className={cn('flex flex-wrap items-center gap-4', className)}>
      {/* Category Dropdown */}
      <div className="relative">
        <select
          value={filter.category ?? ''}
          onChange={(e) => handleChange('category', e.target.value)}
          className="input pr-10 min-w-[180px] appearance-none cursor-pointer"
        >
          {PRODUCT_CATEGORIES.map((cat) => (
            <option key={cat.value} value={cat.value}>
              {cat.label}
            </option>
          ))}
        </select>
        <span className="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-text-muted pointer-events-none">
          expand_more
        </span>
      </div>

      {/* Season Dropdown */}
      <div className="relative">
        <select
          value={filter.season ?? ''}
          onChange={(e) => handleChange('season', e.target.value)}
          className="input pr-10 min-w-[180px] appearance-none cursor-pointer"
        >
          {SEASONS.map((season) => (
            <option key={season.value} value={season.value}>
              {season.label}
            </option>
          ))}
        </select>
        <span className="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-text-muted pointer-events-none">
          expand_more
        </span>
      </div>

      {/* Status Dropdown */}
      <div className="relative">
        <select
          value={filter.status ?? ''}
          onChange={(e) => handleChange('status', e.target.value)}
          className="input pr-10 min-w-[160px] appearance-none cursor-pointer"
        >
          {PRODUCT_STATUS.map((status) => (
            <option key={status.value} value={status.value}>
              {status.label}
            </option>
          ))}
        </select>
        <span className="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-text-muted pointer-events-none">
          expand_more
        </span>
      </div>

      {/* Search Input */}
      <div className="relative flex-1 min-w-[200px]">
        <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-text-muted">
          search
        </span>
        <input
          type="text"
          placeholder="Ürün ara..."
          value={filter.search ?? ''}
          onChange={(e) => handleChange('search', e.target.value)}
          className="input pl-10"
        />
      </div>
    </div>
  );
};
