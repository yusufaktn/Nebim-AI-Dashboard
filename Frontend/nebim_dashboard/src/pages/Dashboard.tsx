import { useEffect, useState } from 'react';
import { 
  Paper, 
  Title, 
  Text, 
  Grid, 
  Card, 
  Group, 
  Badge, 
  Center, 
  Stack, 
  ThemeIcon, 
  Progress,
  Box,
  Transition,
  rem,
  RingProgress,
  SimpleGrid,
  Skeleton
} from '@mantine/core';
import { 
  IconCurrencyLira, 
  IconShoppingCart, 
  IconPackage, 
  IconAlertTriangle,
  IconTrendingUp,
  IconChartBar,
  IconUsers,
  IconArrowUpRight,
  IconArrowDownRight,
  IconBox,
  IconClock
} from '@tabler/icons-react';
import { dashboardService } from '../services';
import type { DashboardResponse, LowStockAlert } from '../types/api';

// Animated stat card component
interface StatCardProps {
  title: string;
  value: string;
  change: number;
  icon: React.ComponentType<{ size?: number | string }>;
  color: string;
  delay: number;
}

const StatCard = ({ title, value, change, icon: Icon, color, delay }: StatCardProps) => {
  const [mounted, setMounted] = useState(false);
  
  useEffect(() => {
    const timer = setTimeout(() => setMounted(true), delay);
    return () => clearTimeout(timer);
  }, [delay]);

  return (
    <Transition mounted={mounted} transition="slide-up" duration={400}>
      {(styles) => (
        <Card 
          style={styles} 
          shadow="sm" 
          padding="lg" 
          radius="lg" 
          withBorder
        >
          <Group justify="space-between" mb="md">
            <Text size="sm" c="dimmed" fw={500} tt="uppercase" style={{ letterSpacing: 0.5 }}>
              {title}
            </Text>
            <ThemeIcon 
              color={color} 
              variant="light" 
              size={44} 
              radius="md"
            >
              <Icon size={22} />
            </ThemeIcon>
          </Group>
          
          <Group align="flex-end" gap="xs">
            <Text size={rem(28)} fw={700} style={{ lineHeight: 1 }}>
              {value}
            </Text>
          </Group>
          
          {change !== 0 && (
            <Group gap={4} mt="md">
              <ThemeIcon 
                size={20} 
                radius="xl" 
                color={change > 0 ? 'teal' : 'red'} 
                variant="light"
              >
                {change > 0 ? <IconArrowUpRight size={14} /> : <IconArrowDownRight size={14} />}
              </ThemeIcon>
              <Text size="sm" c={change > 0 ? 'teal' : 'red'} fw={500}>
                {change > 0 ? '+' : ''}{change.toFixed(1)}%
              </Text>
              <Text size="xs" c="dimmed">geçen aya göre</Text>
            </Group>
          )}
        </Card>
      )}
    </Transition>
  );
};

// Low stock item component - Backend LowStockAlertDto ile uyumlu
interface LowStockItemProps {
  alert: LowStockAlert;
  index: number;
}

const LowStockItem = ({ alert, index }: LowStockItemProps) => {
  const [mounted, setMounted] = useState(false);
  const isCritical = alert.severity === 'Critical';
  const percentage = Math.min((alert.currentQuantity / 10) * 100, 100); // 10 = default threshold
  
  useEffect(() => {
    const timer = setTimeout(() => setMounted(true), 100 + index * 50);
    return () => clearTimeout(timer);
  }, [index]);

  return (
    <Transition mounted={mounted} transition="fade" duration={300}>
      {(styles) => (
        <Paper 
          style={{ 
            ...styles,
            borderLeft: `3px solid var(--mantine-color-${isCritical ? 'red' : 'orange'}-5)`,
          }} 
          p="md" 
          radius="md" 
          withBorder
        >
          <Group justify="space-between" wrap="nowrap">
            <Box style={{ flex: 1, minWidth: 0 }}>
              <Group gap="xs" wrap="nowrap">
                <Text size="sm" fw={600} truncate>
                  {alert.productName}
                </Text>
                {isCritical && (
                  <Badge size="xs" color="red" variant="filled">
                    Kritik
                  </Badge>
                )}
              </Group>
              <Text size="xs" c="dimmed" mt={2}>
                {alert.productCode} • {alert.warehouseName}
              </Text>
            </Box>
            
            <Box style={{ textAlign: 'right' }}>
              <Group gap={4} justify="flex-end">
                <RingProgress
                  size={36}
                  thickness={3}
                  roundCaps
                  sections={[
                    { value: percentage, color: isCritical ? 'red' : 'orange' },
                  ]}
                  label={
                    <Center>
                      <IconBox size={14} />
                    </Center>
                  }
                />
                <Box>
                  <Text size="sm" fw={600} c={isCritical ? 'red' : 'orange'}>
                    {alert.currentQuantity}
                  </Text>
                  <Text size="xs" c="dimmed">
                    adet
                  </Text>
                </Box>
              </Group>
            </Box>
          </Group>
        </Paper>
      )}
    </Transition>
  );
};

export const Dashboard = () => {
  const [dashboardData, setDashboardData] = useState<DashboardResponse | null>(null);
  const [lowStockAlerts, setLowStockAlerts] = useState<LowStockAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [headerMounted, setHeaderMounted] = useState(false);

  useEffect(() => {
    setHeaderMounted(true);
  }, []);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [dashboard, alerts] = await Promise.all([
          dashboardService.getDashboard(),
          dashboardService.getLowStockAlerts(),
        ]);
        setDashboardData(dashboard);
        setLowStockAlerts(alerts);
      } catch (err) {
        console.error('Dashboard veri yüklenirken hata:', err);
        setError('Veriler yüklenirken bir hata oluştu.');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'TRY',
      minimumFractionDigits: 0,
    }).format(value);
  };

  if (error) {
    return (
      <Center h={400}>
        <Paper p="xl" radius="md" withBorder>
          <Stack align="center" gap="md">
            <ThemeIcon size={60} radius="xl" color="red" variant="light">
              <IconAlertTriangle size={30} />
            </ThemeIcon>
            <Text c="red" size="lg" fw={500}>{error}</Text>
          </Stack>
        </Paper>
      </Center>
    );
  }

  // Backend DashboardResponse yapısına uyumlu stats
  const stats = [
    {
      title: 'Günlük Satış',
      value: formatCurrency(dashboardData?.dailySales?.todayTotal || 0),
      change: dashboardData?.dailySales?.changePercentage || 0,
      icon: IconCurrencyLira,
      color: 'blue',
    },
    {
      title: 'Aylık Satış',
      value: formatCurrency(dashboardData?.dailySales?.monthlyTotal || 0),
      change: 0,
      icon: IconShoppingCart,
      color: 'teal',
    },
    {
      title: 'Toplam Ürün',
      value: dashboardData?.stockSummary?.totalProducts?.toString() || '0',
      change: 0,
      icon: IconPackage,
      color: 'violet',
    },
    {
      title: 'Düşük Stok',
      value: dashboardData?.stockSummary?.lowStockCount?.toString() || '0',
      change: 0,
      icon: IconAlertTriangle,
      color: 'orange',
    },
  ];

  return (
    <Stack gap="xl">
      {/* Page Header */}
      <Transition mounted={headerMounted} transition="fade-down" duration={400}>
        {(styles) => (
          <Box style={styles}>
            <Group justify="space-between" align="flex-start">
              <Box>
                <Group gap="md" mb={4}>
                  <ThemeIcon 
                    size={42} 
                    radius="xl" 
                    variant="gradient" 
                    gradient={{ from: 'blue', to: 'cyan', deg: 90 }}
                  >
                    <IconChartBar size={22} />
                  </ThemeIcon>
                  <Title order={2} fw={700}>Dashboard</Title>
                </Group>
                <Text c="dimmed" size="sm" ml={54}>
                  Hoş geldiniz! İşte günlük özet raporunuz.
                </Text>
              </Box>
              
              <Paper p="sm" radius="md" withBorder>
                <Group gap="xs">
                  <IconClock size={16} color="var(--mantine-color-dimmed)" />
                  <Text size="xs" c="dimmed">
                    Son güncelleme: {new Date().toLocaleTimeString('tr-TR')}
                  </Text>
                </Group>
              </Paper>
            </Group>
          </Box>
        )}
      </Transition>

      {/* KPI Cards */}
      {loading ? (
        <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }}>
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} height={140} radius="lg" />
          ))}
        </SimpleGrid>
      ) : (
        <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }}>
          {stats.map((stat, index) => (
            <StatCard key={stat.title} {...stat} delay={100 + index * 100} />
          ))}
        </SimpleGrid>
      )}

      <Grid>
        {/* Low Stock Alerts */}
        <Grid.Col span={{ base: 12, lg: 7 }}>
          <Transition mounted={!loading} transition="slide-right" duration={500} enterDelay={400}>
            {(styles) => (
              <Paper style={styles} shadow="sm" p="lg" radius="lg" withBorder h="100%">
                <Group justify="space-between" mb="lg">
                  <Group gap="sm">
                    <ThemeIcon size={36} radius="md" color="orange" variant="light">
                      <IconAlertTriangle size={20} />
                    </ThemeIcon>
                    <Box>
                      <Title order={4}>Düşük Stok Uyarıları</Title>
                      <Text size="xs" c="dimmed">Stok seviyesi kritik ürünler</Text>
                    </Box>
                  </Group>
                  <Badge 
                    size="lg" 
                    color="orange" 
                    variant="light"
                    leftSection={<IconBox size={14} />}
                  >
                    {lowStockAlerts.length} ürün
                  </Badge>
                </Group>
                
                {lowStockAlerts.length > 0 ? (
                  <Stack gap="sm">
                    {lowStockAlerts.slice(0, 5).map((alert, index) => (
                      <LowStockItem key={index} alert={alert} index={index} />
                    ))}
                  </Stack>
                ) : (
                  <Center h={200}>
                    <Stack align="center" gap="sm">
                      <ThemeIcon size={50} radius="xl" color="green" variant="light">
                        <IconPackage size={26} />
                      </ThemeIcon>
                      <Text c="dimmed">Tüm stoklar yeterli seviyede</Text>
                    </Stack>
                  </Center>
                )}
              </Paper>
            )}
          </Transition>
        </Grid.Col>

        {/* Top Categories */}
        <Grid.Col span={{ base: 12, lg: 5 }}>
          <Transition mounted={!loading} transition="slide-left" duration={500} enterDelay={500}>
            {(styles) => (
              <Paper style={styles} shadow="sm" p="lg" radius="lg" withBorder h="100%">
                <Group gap="sm" mb="lg">
                  <ThemeIcon size={36} radius="md" color="violet" variant="light">
                    <IconTrendingUp size={20} />
                  </ThemeIcon>
                  <Box>
                    <Title order={4}>En Çok Satan Ürünler</Title>
                    <Text size="xs" c="dimmed">Bu haftanın performansı</Text>
                  </Box>
                </Group>
                
                {dashboardData?.topProducts && dashboardData.topProducts.length > 0 ? (
                  <Stack gap="md">
                    {dashboardData.topProducts.map((product, index) => {
                      const maxAmount = Math.max(...dashboardData.topProducts!.map(p => p.totalAmount));
                      const percentage = (product.totalAmount / maxAmount) * 100;
                      
                      return (
                        <Box key={index}>
                          <Group justify="space-between" mb={4}>
                            <Group gap="xs">
                              <Text size="sm" fw={500} truncate style={{ maxWidth: 150 }}>
                                {product.productName}
                              </Text>
                              <Badge size="xs" variant="dot" color="violet">
                                {product.totalQuantity} adet
                              </Badge>
                            </Group>
                            <Text size="sm" fw={600}>
                              {formatCurrency(product.totalAmount)}
                            </Text>
                          </Group>
                          <Progress 
                            value={percentage} 
                            size="sm" 
                            radius="xl" 
                            color="violet"
                          />
                        </Box>
                      );
                    })}
                  </Stack>
                ) : (
                  <Center h={200}>
                    <Stack align="center" gap="sm">
                      <ThemeIcon size={50} radius="xl" color="gray" variant="light">
                        <IconChartBar size={26} />
                      </ThemeIcon>
                      <Text c="dimmed">Henüz veri yok</Text>
                    </Stack>
                  </Center>
                )}
              </Paper>
            )}
          </Transition>
        </Grid.Col>
      </Grid>

      {/* Quick Stats Row */}
      <Transition mounted={!loading} transition="slide-up" duration={500} enterDelay={600}>
        {(styles) => (
          <SimpleGrid style={styles} cols={{ base: 1, sm: 3 }}>
            <Paper p="lg" radius="lg" withBorder>
              <Group>
                <RingProgress
                  size={80}
                  thickness={8}
                  roundCaps
                  sections={[
                    { value: 65, color: 'blue' },
                    { value: 20, color: 'cyan' },
                    { value: 15, color: 'violet' },
                  ]}
                  label={
                    <Center>
                      <IconUsers size={20} />
                    </Center>
                  }
                />
                <Box>
                  <Text size="xs" c="dimmed" tt="uppercase" fw={500}>Aktif Müşteriler</Text>
                  <Text size="xl" fw={700}>1,234</Text>
                  <Text size="xs" c="teal">+12% bu ay</Text>
                </Box>
              </Group>
            </Paper>

            <Paper p="lg" radius="lg" withBorder>
              <Group>
                <RingProgress
                  size={80}
                  thickness={8}
                  roundCaps
                  sections={[
                    { value: 78, color: 'teal' },
                  ]}
                  label={
                    <Center>
                      <Text size="xs" fw={700}>78%</Text>
                    </Center>
                  }
                />
                <Box>
                  <Text size="xs" c="dimmed" tt="uppercase" fw={500}>Sipariş Tamamlama</Text>
                  <Text size="xl" fw={700}>945</Text>
                  <Text size="xs" c="teal">+8% geçen haftaya göre</Text>
                </Box>
              </Group>
            </Paper>

            <Paper p="lg" radius="lg" withBorder>
              <Group>
                <RingProgress
                  size={80}
                  thickness={8}
                  roundCaps
                  sections={[
                    { value: 92, color: 'violet' },
                  ]}
                  label={
                    <Center>
                      <Text size="xs" fw={700}>92%</Text>
                    </Center>
                  }
                />
                <Box>
                  <Text size="xs" c="dimmed" tt="uppercase" fw={500}>Müşteri Memnuniyeti</Text>
                  <Text size="xl" fw={700}>4.8/5</Text>
                  <Text size="xs" c="dimmed">Son 30 gün</Text>
                </Box>
              </Group>
            </Paper>
          </SimpleGrid>
        )}
      </Transition>
    </Stack>
  );
};

