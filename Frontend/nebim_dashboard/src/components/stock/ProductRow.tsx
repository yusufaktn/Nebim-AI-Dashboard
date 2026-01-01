import { cn, formatCurrency, getStatusLabel, getStatusColor } from '../../utils/formatters';
import type { Product } from '../../types';

interface ProductRowProps {
  product: Product;
  className?: string;
}

export const ProductRow = ({ product, className }: ProductRowProps) => {
  const stock = product.stock ?? product.quantity ?? 0;
  const minStock = product.minStock ?? 5;
  const salesSpeed = product.salesSpeed ?? 50;
  const price = product.price ?? product.salePrice ?? 0;
  const status = product.status ?? 'active';
  const name = product.name ?? product.productName;
  const code = product.code ?? product.productCode;
  const category = product.category ?? product.categoryName ?? '';
  const season = product.season ?? '';
  
  return (
    <tr className={cn('table-row', className)}>
      {/* Product Info */}
      <td className="table-cell">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-lg overflow-hidden bg-background-light dark:bg-background-dark flex-shrink-0">
            {product.image ? (
              <img
                src={product.image}
                alt={name}
                className="w-full h-full object-cover"
              />
            ) : (
              <div className="w-full h-full flex items-center justify-center">
                <span className="material-symbols-outlined text-text-muted">
                  image
                </span>
              </div>
            )}
          </div>
          <div>
            <p className="font-medium text-text-main dark:text-text-dark">
              {name}
            </p>
            <p className="text-xs text-text-secondary dark:text-text-dark-secondary">
              {code}
            </p>
          </div>
        </div>
      </td>

      {/* Category */}
      <td className="table-cell">
        <span className="badge bg-primary/10 text-primary dark:bg-primary/20">
          {category}
        </span>
      </td>

      {/* Season */}
      <td className="table-cell text-text-secondary dark:text-text-dark-secondary">
        {season}
      </td>

      {/* Stock */}
      <td className="table-cell">
        <div>
          <p className={cn(
            'font-medium',
            stock <= minStock
              ? 'text-danger'
              : 'text-text-main dark:text-text-dark'
          )}>
            {stock} adet
          </p>
          <p className="text-xs text-text-secondary dark:text-text-dark-secondary">
            Min: {minStock}
          </p>
        </div>
      </td>

      {/* Sales Speed */}
      <td className="table-cell">
        <div className="w-24">
          <div className="flex items-center justify-between text-xs mb-1">
            <span className="text-text-secondary dark:text-text-dark-secondary">Satış Hızı</span>
            <span className="font-medium text-text-main dark:text-text-dark">{salesSpeed}%</span>
          </div>
          <div className="progress-bar">
            <div
              className={cn(
                'progress-bar-fill',
                salesSpeed >= 80 ? 'bg-success' :
                salesSpeed >= 50 ? 'bg-primary' :
                salesSpeed >= 30 ? 'bg-warning' : 'bg-danger'
              )}
              style={{ width: `${salesSpeed}%` }}
            />
          </div>
        </div>
      </td>

      {/* Price */}
      <td className="table-cell font-medium text-text-main dark:text-text-dark">
        {formatCurrency(price)}
      </td>

      {/* Status */}
      <td className="table-cell">
        <span className={cn('badge', getStatusColor(status))}>
          {getStatusLabel(status)}
        </span>
      </td>

      {/* Actions */}
      <td className="table-cell">
        <div className="flex items-center gap-2">
          <button className="p-2 rounded-lg hover:bg-surface-hover dark:hover:bg-background-dark transition-colors">
            <span className="material-symbols-outlined text-text-secondary dark:text-text-dark-secondary text-xl">
              visibility
            </span>
          </button>
          <button className="p-2 rounded-lg hover:bg-surface-hover dark:hover:bg-background-dark transition-colors">
            <span className="material-symbols-outlined text-text-secondary dark:text-text-dark-secondary text-xl">
              edit
            </span>
          </button>
          <button className="p-2 rounded-lg hover:bg-surface-hover dark:hover:bg-background-dark transition-colors">
            <span className="material-symbols-outlined text-text-secondary dark:text-text-dark-secondary text-xl">
              more_vert
            </span>
          </button>
        </div>
      </td>
    </tr>
  );
};
