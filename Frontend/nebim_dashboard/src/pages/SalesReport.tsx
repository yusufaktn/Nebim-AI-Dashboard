import { useState, useEffect } from 'react';
import {
  Paper,
  Title,
  Text,
  Group,
  Box,
  Stack,
  ThemeIcon,
  Transition,
  Select,
  Table,
  Badge,
  Skeleton,
  Alert,
  SimpleGrid,
  ActionIcon,
  Tooltip,
  Pagination,
  rem,
  NumberFormatter,
  TextInput,
} from '@mantine/core';
import {
  IconChartLine,
  IconReceipt,
  IconShoppingCart,
  IconCurrencyLira,
  IconTrendingUp,
  IconRefresh,
  IconAlertCircle,
  IconCalendar,
  IconBuilding,
} from '@tabler/icons-react';
import { salesService, type StoreInfo } from '../services/salesService';
import type { NebimSaleDto, SalesFilterRequest } from '../types/api';

// Stat Card bileşeni
interface StatCardProps {
  title: string;
  value: string | number;
  icon: React.ReactNode;
  color: string;
  suffix?: string;
}

const StatCard = ({ title, value, icon, color, suffix }: StatCardProps) => (
  <Paper shadow="sm" radius="lg" p="lg" withBorder>
    <Group justify="space-between">
      <Box>
        <Text c="dimmed" size="xs" tt="uppercase" fw={600}>
          {title}
        </Text>
        <Group gap="xs" mt={4}>
          <Text size="xl" fw={700}>
            {typeof value === 'number' ? (
              <NumberFormatter value={value} thousandSeparator="." decimalSeparator="," />
            ) : (
              value
            )}
          </Text>
          {suffix && <Text size="sm" c="dimmed">{suffix}</Text>}
        </Group>
      </Box>
      <ThemeIcon size={48} radius="xl" variant="light" color={color}>
        {icon}
      </ThemeIcon>
    </Group>
  </Paper>
);

export const SalesReport = () => {
  const [mounted, setMounted] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Filtreler - string olarak tarih
  const formatDateForInput = (date: Date) => date.toISOString().split('T')[0];
  const [startDate, setStartDate] = useState(formatDateForInput(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000))); // Son 30 gün
  const [endDate, setEndDate] = useState(formatDateForInput(new Date()));
  const [selectedStore, setSelectedStore] = useState<string | null>(null);
  const [stores, setStores] = useState<StoreInfo[]>([]);
  
  // Veriler
  const [sales, setSales] = useState<NebimSaleDto[]>([]);
  const [totalAmount, setTotalAmount] = useState(0);
  const [topProducts, setTopProducts] = useState<NebimSaleDto[]>([]);
  const [dailySales, setDailySales] = useState<Record<string, number>>({});
  
  // Sayfalama
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 15;

  useEffect(() => {
    setMounted(true);
    loadStores();
  }, []);

  useEffect(() => {
    loadData();
  }, [startDate, endDate, selectedStore, page]);

  const loadStores = async () => {
    try {
      const storeList = await salesService.getStores();
      setStores(storeList);
    } catch (err) {
      console.error('Mağaza listesi yüklenemedi:', err);
    }
  };

  const loadData = async () => {
    if (!startDate || !endDate) return;
    
    setLoading(true);
    setError(null);
    
    try {
      const filter: SalesFilterRequest = {
        page,
        pageSize,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        storeCode: selectedStore ?? undefined,
      };
      
      const startStr = new Date(startDate).toISOString();
      const endStr = new Date(endDate).toISOString();
      
      // Paralel API çağrıları
      const [salesResult, total, topProds, daily] = await Promise.all([
        salesService.getSales(filter),
        salesService.getTotalSalesAmount(startStr, endStr),
        salesService.getTopSellingProducts(startStr, endStr, 5),
        salesService.getDailySales(startStr, endStr),
      ]);
      
      setSales(salesResult.items);
      setTotalPages(salesResult.pagination.totalPages);
      setTotalAmount(total);
      setTopProducts(topProds);
      setDailySales(daily);
    } catch (err) {
      console.error('Veriler yüklenemedi:', err);
      setError('Satış verileri yüklenirken bir hata oluştu. Lütfen tekrar deneyin.');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getPaymentBadgeColor = (method?: string) => {
    switch (method) {
      case 'Kredi Kartı': return 'blue';
      case 'Nakit': return 'green';
      case 'Banka Kartı': return 'cyan';
      case 'Havale/EFT': return 'orange';
      default: return 'gray';
    }
  };

  // Hesaplamalar
  const avgDailySales = Object.keys(dailySales).length > 0 
    ? totalAmount / Object.keys(dailySales).length 
    : 0;

  return (
    <Stack gap="xl">
      {/* Page Header */}
      <Transition mounted={mounted} transition="fade-down" duration={400}>
        {(styles) => (
          <Box style={styles}>
            <Group justify="space-between">
              <Group gap="md">
                <ThemeIcon 
                  size={42} 
                  radius="xl" 
                  variant="gradient" 
                  gradient={{ from: 'teal', to: 'green', deg: 90 }}
                >
                  <IconChartLine size={22} />
                </ThemeIcon>
                <Box>
                  <Title order={2} fw={700}>Satış Raporu</Title>
                  <Text c="dimmed" size="sm">Satış performansınızı detaylı analiz edin</Text>
                </Box>
              </Group>
              
              <Tooltip label="Verileri Yenile">
                <ActionIcon 
                  variant="light" 
                  size="lg" 
                  radius="xl"
                  onClick={loadData}
                  loading={loading}
                >
                  <IconRefresh size={18} />
                </ActionIcon>
              </Tooltip>
            </Group>
          </Box>
        )}
      </Transition>

      {/* Filtreler */}
      <Transition mounted={mounted} transition="slide-up" duration={400} enterDelay={100}>
        {(styles) => (
          <Paper style={styles} shadow="sm" radius="lg" p="md" withBorder>
            <Group>
              <TextInput
                type="date"
                label="Başlangıç Tarihi"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                leftSection={<IconCalendar size={16} />}
                style={{ flex: 1 }}
              />
              <TextInput
                type="date"
                label="Bitiş Tarihi"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                leftSection={<IconCalendar size={16} />}
                style={{ flex: 1 }}
              />
              
              <Select
                label="Mağaza"
                placeholder="Tüm mağazalar"
                leftSection={<IconBuilding size={16} />}
                data={[
                  { value: '', label: 'Tüm Mağazalar' },
                  ...stores.map(s => ({ value: s.code, label: s.name }))
                ]}
                value={selectedStore}
                onChange={setSelectedStore}
                clearable
                style={{ minWidth: rem(250) }}
              />
            </Group>
          </Paper>
        )}
      </Transition>

      {/* Hata Mesajı */}
      {error && (
        <Alert icon={<IconAlertCircle size={16} />} color="red" variant="light">
          {error}
        </Alert>
      )}

      {/* İstatistik Kartları */}
      <Transition mounted={mounted} transition="slide-up" duration={400} enterDelay={200}>
        {(styles) => (
          <SimpleGrid style={styles} cols={{ base: 1, sm: 2, lg: 4 }} spacing="md">
            {loading ? (
              <>
                <Skeleton height={100} radius="lg" />
                <Skeleton height={100} radius="lg" />
                <Skeleton height={100} radius="lg" />
                <Skeleton height={100} radius="lg" />
              </>
            ) : (
              <>
                <StatCard
                  title="Toplam Ciro"
                  value={totalAmount}
                  icon={<IconCurrencyLira size={24} />}
                  color="teal"
                  suffix="₺"
                />
                <StatCard
                  title="Satış Adedi"
                  value={sales.length}
                  icon={<IconReceipt size={24} />}
                  color="blue"
                />
                <StatCard
                  title="Ürün Satışı"
                  value={sales.reduce((sum, s) => sum + s.quantity, 0)}
                  icon={<IconShoppingCart size={24} />}
                  color="violet"
                  suffix="adet"
                />
                <StatCard
                  title="Günlük Ortalama"
                  value={Math.round(avgDailySales)}
                  icon={<IconTrendingUp size={24} />}
                  color="orange"
                  suffix="₺"
                />
              </>
            )}
          </SimpleGrid>
        )}
      </Transition>

      {/* En Çok Satan Ürünler */}
      <Transition mounted={mounted} transition="slide-up" duration={400} enterDelay={300}>
        {(styles) => (
          <Paper style={styles} shadow="sm" radius="lg" p="md" withBorder>
            <Title order={4} mb="md">
              <Group gap="xs">
                <IconTrendingUp size={20} />
                En Çok Satan Ürünler
              </Group>
            </Title>
            
            {loading ? (
              <Stack gap="xs">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} height={40} radius="md" />
                ))}
              </Stack>
            ) : topProducts.length > 0 ? (
              <Table striped highlightOnHover>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>#</Table.Th>
                    <Table.Th>Ürün</Table.Th>
                    <Table.Th>Adet</Table.Th>
                    <Table.Th style={{ textAlign: 'right' }}>Tutar</Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {topProducts.map((product, index) => (
                    <Table.Tr key={product.productCode + index}>
                      <Table.Td>
                        <Badge 
                          variant={index < 3 ? 'filled' : 'light'}
                          color={index === 0 ? 'yellow' : index === 1 ? 'gray' : index === 2 ? 'orange' : 'blue'}
                        >
                          {index + 1}
                        </Badge>
                      </Table.Td>
                      <Table.Td>
                        <Text size="sm" fw={500}>{product.productName}</Text>
                        <Text size="xs" c="dimmed">{product.productCode}</Text>
                      </Table.Td>
                      <Table.Td>
                        <Badge variant="light">{product.quantity} adet</Badge>
                      </Table.Td>
                      <Table.Td style={{ textAlign: 'right' }}>
                        <Text fw={600} c="teal">
                          <NumberFormatter value={product.totalAmount} thousandSeparator="." decimalSeparator="," suffix=" ₺" />
                        </Text>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            ) : (
              <Text c="dimmed" ta="center" py="xl">Bu dönemde satış verisi bulunamadı.</Text>
            )}
          </Paper>
        )}
      </Transition>

      {/* Satış Listesi */}
      <Transition mounted={mounted} transition="slide-up" duration={400} enterDelay={400}>
        {(styles) => (
          <Paper style={styles} shadow="sm" radius="lg" p="md" withBorder>
            <Title order={4} mb="md">
              <Group gap="xs">
                <IconReceipt size={20} />
                Satış Detayları
              </Group>
            </Title>
            
            {loading ? (
              <Stack gap="xs">
                {[...Array(10)].map((_, i) => (
                  <Skeleton key={i} height={50} radius="md" />
                ))}
              </Stack>
            ) : sales.length > 0 ? (
              <>
                <Table striped highlightOnHover>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Fiş No</Table.Th>
                      <Table.Th>Tarih</Table.Th>
                      <Table.Th>Mağaza</Table.Th>
                      <Table.Th>Ürün</Table.Th>
                      <Table.Th>Adet</Table.Th>
                      <Table.Th>Ödeme</Table.Th>
                      <Table.Th style={{ textAlign: 'right' }}>Tutar</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {sales.map((sale, index) => (
                      <Table.Tr key={sale.receiptNumber + index}>
                        <Table.Td>
                          <Text size="sm" ff="monospace">{sale.receiptNumber}</Text>
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm">{formatDate(sale.saleDate)}</Text>
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm">{sale.storeName || sale.storeCode}</Text>
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm" fw={500}>{sale.productName}</Text>
                          {(sale.colorName || sale.sizeName) && (
                            <Text size="xs" c="dimmed">
                              {[sale.colorName, sale.sizeName].filter(Boolean).join(' / ')}
                            </Text>
                          )}
                        </Table.Td>
                        <Table.Td>
                          <Badge variant="light" size="sm">{sale.quantity}</Badge>
                        </Table.Td>
                        <Table.Td>
                          <Badge 
                            variant="light" 
                            size="sm"
                            color={getPaymentBadgeColor(sale.paymentMethod)}
                          >
                            {sale.paymentMethod || 'Bilinmiyor'}
                          </Badge>
                        </Table.Td>
                        <Table.Td style={{ textAlign: 'right' }}>
                          <Text fw={600} c="teal">
                            <NumberFormatter value={sale.totalAmount} thousandSeparator="." decimalSeparator="," suffix=" ₺" />
                          </Text>
                          {sale.discountAmount > 0 && (
                            <Text size="xs" c="red">
                              -<NumberFormatter value={sale.discountAmount} thousandSeparator="." decimalSeparator="," suffix=" ₺" />
                            </Text>
                          )}
                        </Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
                
                {/* Sayfalama */}
                {totalPages > 1 && (
                  <Group justify="center" mt="lg">
                    <Pagination 
                      value={page} 
                      onChange={setPage} 
                      total={totalPages}
                      radius="md"
                      withEdges
                    />
                  </Group>
                )}
              </>
            ) : (
              <Text c="dimmed" ta="center" py="xl">
                Seçilen kriterlere uygun satış bulunamadı.
              </Text>
            )}
          </Paper>
        )}
      </Transition>

      {/* CSS Animation for pulse */}
      <style>{`
        @keyframes pulse {
          0%, 100% { transform: scale(1); }
          50% { transform: scale(1.05); }
        }
      `}</style>
    </Stack>
  );
};

export default SalesReport;