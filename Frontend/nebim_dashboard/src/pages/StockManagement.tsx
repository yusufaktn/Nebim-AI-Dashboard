import { useState, useEffect, useCallback } from 'react';
import {
  Paper,
  Title,
  Text,
  Group,
  Button,
  TextInput,
  Select,
  Table,
  Badge,
  Box,
  Stack,
  ThemeIcon,
  Transition,
  Center,
  Pagination as MantinePagination,
  ActionIcon,
  Menu,
  Tooltip,
  Progress,
  Avatar,
  Skeleton,
} from '@mantine/core';
import {
  IconSearch,
  IconFilter,
  IconDownload,
  IconPlus,
  IconPackage,
  IconDotsVertical,
  IconEdit,
  IconTrash,
  IconEye,
  IconBox,
  IconRefresh,
  IconTrendingUp,
  IconTrendingDown
} from '@tabler/icons-react';
import { stockService } from '../services';
import type { NebimStockDto, StockFilterRequest } from '../types/api';

// Debounce utility
const debounce = <T extends StockFilterRequest>(fn: (arg: T) => void, delay: number) => {
  let timeoutId: ReturnType<typeof setTimeout>;
  return (arg: T) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => fn(arg), delay);
  };
};

export const StockManagement = () => {
  const [stocks, setStocks] = useState<NebimStockDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [headerMounted, setHeaderMounted] = useState(false);
  const [tableMounted, setTableMounted] = useState(false);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 10,
    total: 0,
    totalPages: 0,
  });
  const [filter, setFilter] = useState<StockFilterRequest>({
    categoryName: '',
    brandName: '',
    productName: '',
    page: 1,
    pageSize: 10,
  });

  useEffect(() => {
    setHeaderMounted(true);
  }, []);

  const fetchStocks = useCallback(async (currentFilter: StockFilterRequest, page: number) => {
    setLoading(true);
    try {
      const response = await stockService.getStocks({
        ...currentFilter,
        page,
        pageSize: pagination.pageSize,
      });
      setStocks(response.items || []);
      if (response.pagination) {
        setPagination({
          page: response.pagination.page,
          pageSize: response.pagination.pageSize,
          total: response.pagination.totalCount,
          totalPages: response.pagination.totalPages,
        });
      }
      setTimeout(() => setTableMounted(true), 100);
    } catch (error) {
      console.error('Stoklar yüklenirken hata:', error);
      setStocks([]);
    } finally {
      setLoading(false);
    }
  }, [pagination.pageSize]);

  const debouncedFetch = useCallback(
    debounce((f: StockFilterRequest) => {
      fetchStocks(f, 1);
    }, 300),
    [fetchStocks]
  );

  useEffect(() => {
    fetchStocks(filter, pagination.page);
  }, []);

  useEffect(() => {
    if (filter.productName !== undefined) {
      debouncedFetch(filter);
    }
  }, [filter.productName, debouncedFetch]);

  const handleFilterChange = (key: keyof StockFilterRequest, value: string | null) => {
    const newFilter = { ...filter, [key]: value || '' };
    setFilter(newFilter);
    if (key !== 'productName') {
      fetchStocks(newFilter, 1);
    }
  };

  const handlePageChange = (page: number) => {
    fetchStocks(filter, page);
  };

  const getStockStatus = (qty: number, minStock: number = 10) => {
    if (qty <= 0) return { label: 'Stok Yok', color: 'red' };
    if (qty < minStock) return { label: 'Düşük', color: 'orange' };
    if (qty < minStock * 2) return { label: 'Normal', color: 'blue' };
    return { label: 'Yeterli', color: 'green' };
  };

  const categories = [
    { value: '', label: 'Tüm Kategoriler' },
    { value: 'giyim', label: 'Giyim' },
    { value: 'ayakkabi', label: 'Ayakkabı' },
    { value: 'aksesuar', label: 'Aksesuar' },
    { value: 'elektronik', label: 'Elektronik' },
  ];

  const brands = [
    { value: '', label: 'Tüm Markalar' },
  ];

  return (
    <Stack gap="xl">
      {/* Page Header */}
      <Transition mounted={headerMounted} transition="fade-down" duration={400}>
        {(styles) => (
          <Box style={styles}>
            <Group justify="space-between" align="flex-start" wrap="wrap">
              <Box>
                <Group gap="md" mb={4}>
                  <ThemeIcon 
                    size={42} 
                    radius="xl" 
                    variant="gradient" 
                    gradient={{ from: 'violet', to: 'grape', deg: 90 }}
                  >
                    <IconPackage size={22} />
                  </ThemeIcon>
                  <Title order={2} fw={700}>Stok Yönetimi</Title>
                </Group>
                <Text c="dimmed" size="sm" ml={54}>
                  Tüm ürünlerinizi yönetin ve stok durumlarını takip edin
                </Text>
              </Box>
              
              <Group gap="sm">
                <Button 
                  variant="light" 
                  leftSection={<IconDownload size={18} />}
                  radius="md"
                >
                  Dışa Aktar
                </Button>
                <Button 
                  variant="gradient"
                  gradient={{ from: 'violet', to: 'grape' }}
                  leftSection={<IconPlus size={18} />}
                  radius="md"
                >
                  Yeni Ürün
                </Button>
              </Group>
            </Group>
          </Box>
        )}
      </Transition>

      {/* Filters */}
      <Transition mounted={headerMounted} transition="slide-up" duration={400} enterDelay={100}>
        {(styles) => (
          <Paper style={styles} shadow="sm" p="lg" radius="lg" withBorder>
            <Group gap="md" wrap="wrap">
              <TextInput
                placeholder="Ürün ara..."
                leftSection={<IconSearch size={18} />}
                value={filter.productName || ''}
                onChange={(e) => handleFilterChange('productName', e.target.value)}
                style={{ flex: 1, minWidth: 200 }}
                radius="md"
                size="sm"
              />
              
              <Select
                placeholder="Kategori"
                leftSection={<IconFilter size={16} />}
                data={categories}
                value={filter.categoryName || ''}
                onChange={(value) => handleFilterChange('categoryName', value)}
                clearable
                radius="md"
                size="sm"
                w={160}
              />
              
              <Select
                placeholder="Marka"
                data={brands}
                value={filter.brandName || ''}
                onChange={(value) => handleFilterChange('brandName', value)}
                clearable
                radius="md"
                size="sm"
                w={150}
              />
              
              <Tooltip label="Yenile">
                <ActionIcon 
                  variant="light" 
                  size="lg" 
                  radius="md"
                  onClick={() => fetchStocks(filter, pagination.page)}
                >
                  <IconRefresh size={18} />
                </ActionIcon>
              </Tooltip>
            </Group>
          </Paper>
        )}
      </Transition>

      {/* Summary Stats */}
      <Transition mounted={!loading && tableMounted} transition="fade" duration={300}>
        {(styles) => (
          <Group style={styles} gap="md">
            <Paper p="md" radius="lg" withBorder style={{ flex: 1 }}>
              <Group gap="sm">
                <ThemeIcon size={40} radius="md" color="blue" variant="light">
                  <IconBox size={20} />
                </ThemeIcon>
                <Box>
                  <Text size="xs" c="dimmed">Toplam Kayıt</Text>
                  <Text size="lg" fw={700}>{pagination.total}</Text>
                </Box>
              </Group>
            </Paper>
            <Paper p="md" radius="lg" withBorder style={{ flex: 1 }}>
              <Group gap="sm">
                <ThemeIcon size={40} radius="md" color="green" variant="light">
                  <IconTrendingUp size={20} />
                </ThemeIcon>
                <Box>
                  <Text size="xs" c="dimmed">Stokta</Text>
                  <Text size="lg" fw={700}>{stocks.filter(s => s.quantity > 0).length}</Text>
                </Box>
              </Group>
            </Paper>
            <Paper p="md" radius="lg" withBorder style={{ flex: 1 }}>
              <Group gap="sm">
                <ThemeIcon size={40} radius="md" color="orange" variant="light">
                  <IconTrendingDown size={20} />
                </ThemeIcon>
                <Box>
                  <Text size="xs" c="dimmed">Düşük Stok</Text>
                  <Text size="lg" fw={700}>{stocks.filter(s => s.quantity < 10 && s.quantity > 0).length}</Text>
                </Box>
              </Group>
            </Paper>
          </Group>
        )}
      </Transition>

      {/* Table */}
      <Transition mounted={headerMounted} transition="slide-up" duration={400} enterDelay={200}>
        {(styles) => (
          <Paper style={styles} shadow="sm" radius="lg" withBorder>
            {loading ? (
              <Box p="xl">
                <Stack gap="md">
                  {[1, 2, 3, 4, 5].map((i) => (
                    <Skeleton key={i} height={60} radius="md" />
                  ))}
                </Stack>
              </Box>
            ) : stocks.length === 0 ? (
              <Center h={300}>
                <Stack align="center" gap="md">
                  <ThemeIcon size={60} radius="xl" color="gray" variant="light">
                    <IconPackage size={30} />
                  </ThemeIcon>
                  <Text c="dimmed" size="lg">Ürün bulunamadı</Text>
                  <Text c="dimmed" size="sm">Farklı filtreler deneyebilirsiniz</Text>
                </Stack>
              </Center>
            ) : (
              <>
                <Table.ScrollContainer minWidth={800}>
                  <Table verticalSpacing="md" highlightOnHover>
                    <Table.Thead>
                      <Table.Tr>
                        <Table.Th>Ürün</Table.Th>
                        <Table.Th>Kod</Table.Th>
                        <Table.Th>Renk</Table.Th>
                        <Table.Th>Beden</Table.Th>
                        <Table.Th>Kullanılabilir</Table.Th>
                        <Table.Th>Stok</Table.Th>
                        <Table.Th>Durum</Table.Th>
                        <Table.Th w={60}></Table.Th>
                      </Table.Tr>
                    </Table.Thead>
                    <Table.Tbody>
                      {stocks.map((stock, index) => {
                        const status = getStockStatus(stock.quantity);
                        const stockPercentage = Math.min((stock.quantity / 50) * 100, 100);
                        
                        return (
                          <Table.Tr key={`${stock.productCode}-${stock.warehouseCode}-${index}`}>
                            <Table.Td>
                              <Group gap="sm" wrap="nowrap">
                                <Avatar 
                                  size={40} 
                                  radius="md" 
                                  color="violet"
                                  variant="light"
                                >
                                  {stock.productName.charAt(0)}
                                </Avatar>
                                <Box>
                                  <Text size="sm" fw={500} lineClamp={1}>
                                    {stock.productName}
                                  </Text>
                                  <Text size="xs" c="dimmed">
                                    {stock.warehouseName || stock.warehouseCode}
                                  </Text>
                                </Box>
                              </Group>
                            </Table.Td>
                            <Table.Td>
                              <Badge variant="light" color="gray" radius="sm">
                                {stock.productCode}
                              </Badge>
                            </Table.Td>
                            <Table.Td>
                              <Group gap="xs">
                                <Box
                                  w={16}
                                  h={16}
                                  style={{
                                    borderRadius: '50%',
                                    backgroundColor: stock.colorName?.toLowerCase() === 'siyah' ? '#000' : 
                                                    stock.colorName?.toLowerCase() === 'beyaz' ? '#fff' :
                                                    stock.colorName?.toLowerCase() === 'kırmızı' ? '#dc2626' :
                                                    stock.colorName?.toLowerCase() === 'mavi' ? '#3b82f6' : '#9ca3af',
                                    border: '1px solid var(--mantine-color-default-border)',
                                  }}
                                />
                                <Text size="sm">{stock.colorName || '-'}</Text>
                              </Group>
                            </Table.Td>
                            <Table.Td>
                              <Badge variant="outline" color="blue" radius="sm">
                                {stock.sizeName || '-'}
                              </Badge>
                            </Table.Td>
                            <Table.Td>
                              <Text size="sm" fw={500}>
                                {stock.availableQuantity} / {stock.quantity}
                              </Text>
                              <Text size="xs" c="dimmed">Kullanılabilir</Text>
                            </Table.Td>
                            <Table.Td>
                              <Box w={80}>
                                <Text size="xs" fw={500} mb={4}>{stock.quantity} adet</Text>
                                <Progress 
                                  value={stockPercentage} 
                                  size="xs" 
                                  radius="xl"
                                  color={status.color}
                                />
                              </Box>
                            </Table.Td>
                            <Table.Td>
                              <Badge 
                                color={status.color} 
                                variant="light"
                                radius="sm"
                              >
                                {status.label}
                              </Badge>
                            </Table.Td>
                            <Table.Td>
                              <Menu position="bottom-end" withArrow>
                                <Menu.Target>
                                  <ActionIcon variant="subtle" color="gray">
                                    <IconDotsVertical size={16} />
                                  </ActionIcon>
                                </Menu.Target>
                                <Menu.Dropdown>
                                  <Menu.Item leftSection={<IconEye size={14} />}>
                                    Görüntüle
                                  </Menu.Item>
                                  <Menu.Item leftSection={<IconEdit size={14} />}>
                                    Düzenle
                                  </Menu.Item>
                                  <Menu.Divider />
                                  <Menu.Item leftSection={<IconTrash size={14} />} color="red">
                                    Sil
                                  </Menu.Item>
                                </Menu.Dropdown>
                              </Menu>
                            </Table.Td>
                          </Table.Tr>
                        );
                      })}
                    </Table.Tbody>
                  </Table>
                </Table.ScrollContainer>

                {/* Pagination */}
                <Box p="md" style={{ borderTop: '1px solid var(--mantine-color-default-border)' }}>
                  <Group justify="space-between">
                    <Text size="sm" c="dimmed">
                      Toplam {pagination.total} kayıt • Sayfa {pagination.page}/{pagination.totalPages || 1}
                    </Text>
                    <MantinePagination 
                      total={pagination.totalPages || 1} 
                      value={pagination.page} 
                      onChange={handlePageChange}
                      radius="md"
                      size="sm"
                    />
                  </Group>
                </Box>
              </>
            )}
          </Paper>
        )}
      </Transition>
    </Stack>
  );
};
