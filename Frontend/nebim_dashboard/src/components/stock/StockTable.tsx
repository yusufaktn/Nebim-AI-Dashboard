import { cn } from '../../utils/formatters';
import { ProductRow } from './ProductRow';
import type { Product } from '../../types';

interface StockTableProps {
  products: Product[];
  loading?: boolean;
  className?: string;
}

export const StockTable = ({ products, loading, className }: StockTableProps) => {
  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="spinner" />
      </div>
    );
  }

  if (products.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-64 text-center">
        <span className="material-symbols-outlined text-6xl text-text-muted mb-4">
          inventory_2
        </span>
        <p className="text-lg font-medium text-text-main dark:text-text-dark">
          Ürün bulunamadı
        </p>
        <p className="text-sm text-text-secondary dark:text-text-dark-secondary mt-1">
          Filtreleri değiştirerek tekrar deneyin
        </p>
      </div>
    );
  }

  return (
    <div className={cn('overflow-x-auto', className)}>
      <table className="w-full min-w-[900px]">
        <thead>
          <tr className="border-b border-border dark:border-border-dark">
            <th className="table-header px-6 py-4">Ürün</th>
            <th className="table-header px-6 py-4">Kategori</th>
            <th className="table-header px-6 py-4">Sezon</th>
            <th className="table-header px-6 py-4">Stok</th>
            <th className="table-header px-6 py-4">Satış Hızı</th>
            <th className="table-header px-6 py-4">Fiyat</th>
            <th className="table-header px-6 py-4">Durum</th>
            <th className="table-header px-6 py-4">İşlemler</th>
          </tr>
        </thead>
        <tbody>
          {products.map((product) => (
            <ProductRow key={product.productCode ?? product.code ?? product.id} product={product} />
          ))}
        </tbody>
      </table>
    </div>
  );
};
