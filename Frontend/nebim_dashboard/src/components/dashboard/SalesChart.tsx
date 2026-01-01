import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { cn, formatCurrency } from '../../utils/formatters';
import { CHART_COLORS } from '../../utils/constants';
import type { SalesData } from '../../types';

interface SalesChartProps {
  data: SalesData[];
  className?: string;
}

// Custom tooltip component
const CustomTooltip = ({ active, payload, label }: { active?: boolean; payload?: unknown[]; label?: string }) => {
  if (active && payload && payload.length) {
    const data = payload[0] as { payload: SalesData };
    return (
      <div className="bg-surface dark:bg-surface-dark border border-border dark:border-border-dark rounded-lg shadow-dropdown p-3">
        <p className="text-sm font-medium text-text-main dark:text-text-dark mb-2">
          {label}
        </p>
        <div className="space-y-1">
          <p className="text-sm text-text-secondary dark:text-text-dark-secondary">
            Satış: <span className="font-medium text-text-main dark:text-text-dark">{data.payload.sales ?? data.payload.count ?? 0} adet</span>
          </p>
          <p className="text-sm text-text-secondary dark:text-text-dark-secondary">
            Ciro: <span className="font-medium text-primary">{formatCurrency(data.payload.revenue ?? data.payload.amount ?? 0)}</span>
          </p>
        </div>
      </div>
    );
  }
  return null;
};

export const SalesChart = ({ data, className }: SalesChartProps) => {
  return (
    <div className={cn('card', className)}>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="text-lg font-semibold text-text-main dark:text-text-dark">
            Haftalık Satış Grafiği
          </h3>
          <p className="text-sm text-text-secondary dark:text-text-dark-secondary mt-1">
            Son 7 günlük satış performansı
          </p>
        </div>

        {/* Legend */}
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-primary" />
            <span className="text-sm text-text-secondary dark:text-text-dark-secondary">
              Ciro
            </span>
          </div>
        </div>
      </div>

      {/* Chart */}
      <div className="h-[300px]">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart
            data={data}
            margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
          >
            <defs>
              <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor={CHART_COLORS.primary} stopOpacity={0.3} />
                <stop offset="95%" stopColor={CHART_COLORS.primary} stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid
              strokeDasharray="3 3"
              vertical={false}
              stroke="#E5E7EB"
              className="dark:stroke-border-dark"
            />
            <XAxis
              dataKey="day"
              axisLine={false}
              tickLine={false}
              tick={{ fontSize: 12, fill: '#6B7280' }}
              dy={10}
            />
            <YAxis
              axisLine={false}
              tickLine={false}
              tick={{ fontSize: 12, fill: '#6B7280' }}
              tickFormatter={(value) => `${value / 1000}K`}
              dx={-10}
            />
            <Tooltip content={<CustomTooltip />} />
            <Area
              type="monotone"
              dataKey="revenue"
              stroke={CHART_COLORS.primary}
              strokeWidth={2}
              fill="url(#colorRevenue)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};
