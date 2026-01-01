import { cn } from '../../utils/formatters';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  className?: string;
}

export const Pagination = ({
  currentPage,
  totalPages,
  totalItems,
  pageSize,
  onPageChange,
  className,
}: PaginationProps) => {
  const startItem = (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, totalItems);

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (currentPage <= 3) {
        for (let i = 1; i <= 4; i++) {
          pages.push(i);
        }
        pages.push('...');
        pages.push(totalPages);
      } else if (currentPage >= totalPages - 2) {
        pages.push(1);
        pages.push('...');
        for (let i = totalPages - 3; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push('...');
        pages.push(currentPage - 1);
        pages.push(currentPage);
        pages.push(currentPage + 1);
        pages.push('...');
        pages.push(totalPages);
      }
    }

    return pages;
  };

  return (
    <div className={cn('flex flex-col sm:flex-row items-center justify-between gap-4', className)}>
      {/* Info */}
      <p className="text-sm text-text-secondary dark:text-text-dark-secondary">
        <span className="font-medium text-text-main dark:text-text-dark">{startItem}</span>
        {' - '}
        <span className="font-medium text-text-main dark:text-text-dark">{endItem}</span>
        {' / '}
        <span className="font-medium text-text-main dark:text-text-dark">{totalItems}</span>
        {' sonuç gösteriliyor'}
      </p>

      {/* Page buttons */}
      <div className="flex items-center gap-1">
        {/* Previous button */}
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage === 1}
          className={cn(
            'p-2 rounded-lg transition-colors',
            currentPage === 1
              ? 'opacity-50 cursor-not-allowed'
              : 'hover:bg-surface-hover dark:hover:bg-surface-dark'
          )}
        >
          <span className="material-symbols-outlined text-text-secondary dark:text-text-dark-secondary">
            chevron_left
          </span>
        </button>

        {/* Page numbers */}
        {getPageNumbers().map((page, index) => (
          <button
            key={index}
            onClick={() => typeof page === 'number' && onPageChange(page)}
            disabled={page === '...'}
            className={cn(
              'min-w-[40px] h-10 px-3 rounded-lg text-sm font-medium transition-colors',
              page === currentPage
                ? 'bg-primary text-white'
                : page === '...'
                ? 'cursor-default text-text-secondary dark:text-text-dark-secondary'
                : 'text-text-secondary dark:text-text-dark-secondary hover:bg-surface-hover dark:hover:bg-surface-dark'
            )}
          >
            {page}
          </button>
        ))}

        {/* Next button */}
        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage === totalPages}
          className={cn(
            'p-2 rounded-lg transition-colors',
            currentPage === totalPages
              ? 'opacity-50 cursor-not-allowed'
              : 'hover:bg-surface-hover dark:hover:bg-surface-dark'
          )}
        >
          <span className="material-symbols-outlined text-text-secondary dark:text-text-dark-secondary">
            chevron_right
          </span>
        </button>
      </div>
    </div>
  );
};
