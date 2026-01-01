import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Paper,
  Title,
  Text,
  Group,
  Box,
  Stack,
  ThemeIcon,
  Transition,
  Switch,
  Avatar,
  Button,
  Divider,
  Select,
  Badge,
  SimpleGrid,
} from '@mantine/core';
import {
  IconSettings,
  IconPalette,
  IconBell,
  IconUser,
  IconInfoCircle,
  IconMoon,
  IconSun,
  IconLanguage,
  IconMail,
  IconPackage,
  IconChartBar,
  IconLock,
  IconLogout,
  IconEdit,
} from '@tabler/icons-react';
import { useAuthStore } from '../store';
import { useAppTheme } from '../hooks';

export const Settings = () => {
  const navigate = useNavigate();
  const { toggle: toggleColorScheme, isDark } = useAppTheme();
  const { user, logout } = useAuthStore();
  const [mounted, setMounted] = useState(false);

  // Notification settings state
  const [notifications, setNotifications] = useState({
    email: true,
    stock: true,
    sales: false,
  });

  useEffect(() => {
    setMounted(true);
  }, []);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <Stack gap="xl">
      {/* Page Header */}
      <Transition mounted={mounted} transition="fade-down" duration={400}>
        {(styles) => (
          <Box style={styles}>
            <Group gap="md" mb={4}>
              <ThemeIcon 
                size={42} 
                radius="xl" 
                variant="gradient" 
                gradient={{ from: 'blue', to: 'indigo', deg: 90 }}
              >
                <IconSettings size={22} />
              </ThemeIcon>
              <Box>
                <Title order={2} fw={700}>Ayarlar</Title>
                <Text c="dimmed" size="sm">Uygulama tercihlerinizi yönetin</Text>
              </Box>
            </Group>
          </Box>
        )}
      </Transition>

      {/* Settings Grid */}
      <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="lg">
        {/* Appearance */}
        <Transition mounted={mounted} transition="slide-right" duration={400} enterDelay={100}>
          {(styles) => (
            <Paper style={styles} shadow="sm" p="xl" radius="lg" withBorder>
              <Group gap="sm" mb="lg">
                <ThemeIcon size={36} radius="md" color="violet" variant="light">
                  <IconPalette size={20} />
                </ThemeIcon>
                <Title order={4}>Görünüm</Title>
              </Group>

              <Stack gap="md">
                {/* Theme Toggle */}
                <Paper p="md" radius="md" withBorder>
                  <Group justify="space-between">
                    <Group gap="md">
                      <ThemeIcon 
                        size={40} 
                        radius="md" 
                        color={isDark ? 'yellow' : 'blue'} 
                        variant="light"
                      >
                        {isDark ? <IconMoon size={20} /> : <IconSun size={20} />}
                      </ThemeIcon>
                      <Box>
                        <Text size="sm" fw={500}>Tema</Text>
                        <Text size="xs" c="dimmed">
                          {isDark ? 'Koyu tema aktif' : 'Açık tema aktif'}
                        </Text>
                      </Box>
                    </Group>
                    <Switch
                      checked={isDark}
                      onChange={() => toggleColorScheme()}
                      size="lg"
                      color="blue"
                    />
                  </Group>
                </Paper>

                {/* Language */}
                <Paper p="md" radius="md" withBorder>
                  <Group justify="space-between">
                    <Group gap="md">
                      <ThemeIcon size={40} radius="md" color="cyan" variant="light">
                        <IconLanguage size={20} />
                      </ThemeIcon>
                      <Box>
                        <Text size="sm" fw={500}>Dil</Text>
                        <Text size="xs" c="dimmed">Arayüz dili</Text>
                      </Box>
                    </Group>
                    <Select
                      value="tr"
                      data={[
                        { value: 'tr', label: 'Türkçe' },
                        { value: 'en', label: 'English' },
                      ]}
                      w={120}
                      size="sm"
                      radius="md"
                    />
                  </Group>
                </Paper>
              </Stack>
            </Paper>
          )}
        </Transition>

        {/* Notifications */}
        <Transition mounted={mounted} transition="slide-left" duration={400} enterDelay={200}>
          {(styles) => (
            <Paper style={styles} shadow="sm" p="xl" radius="lg" withBorder>
              <Group gap="sm" mb="lg">
                <ThemeIcon size={36} radius="md" color="orange" variant="light">
                  <IconBell size={20} />
                </ThemeIcon>
                <Title order={4}>Bildirimler</Title>
              </Group>

              <Stack gap="md">
                <Paper p="md" radius="md" withBorder>
                  <Group justify="space-between">
                    <Group gap="md">
                      <ThemeIcon size={40} radius="md" color="blue" variant="light">
                        <IconMail size={20} />
                      </ThemeIcon>
                      <Box>
                        <Text size="sm" fw={500}>E-posta Bildirimleri</Text>
                        <Text size="xs" c="dimmed">Önemli güncellemeler için e-posta al</Text>
                      </Box>
                    </Group>
                    <Switch
                      checked={notifications.email}
                      onChange={(e) => setNotifications({ ...notifications, email: e.target.checked })}
                      size="lg"
                      color="blue"
                    />
                  </Group>
                </Paper>

                <Paper p="md" radius="md" withBorder>
                  <Group justify="space-between">
                    <Group gap="md">
                      <ThemeIcon size={40} radius="md" color="orange" variant="light">
                        <IconPackage size={20} />
                      </ThemeIcon>
                      <Box>
                        <Text size="sm" fw={500}>Stok Uyarıları</Text>
                        <Text size="xs" c="dimmed">Düşük stok uyarıları</Text>
                      </Box>
                    </Group>
                    <Switch
                      checked={notifications.stock}
                      onChange={(e) => setNotifications({ ...notifications, stock: e.target.checked })}
                      size="lg"
                      color="orange"
                    />
                  </Group>
                </Paper>

                <Paper p="md" radius="md" withBorder>
                  <Group justify="space-between">
                    <Group gap="md">
                      <ThemeIcon size={40} radius="md" color="green" variant="light">
                        <IconChartBar size={20} />
                      </ThemeIcon>
                      <Box>
                        <Text size="sm" fw={500}>Satış Raporları</Text>
                        <Text size="xs" c="dimmed">Günlük satış özeti</Text>
                      </Box>
                    </Group>
                    <Switch
                      checked={notifications.sales}
                      onChange={(e) => setNotifications({ ...notifications, sales: e.target.checked })}
                      size="lg"
                      color="green"
                    />
                  </Group>
                </Paper>
              </Stack>
            </Paper>
          )}
        </Transition>

        {/* Account */}
        <Transition mounted={mounted} transition="slide-right" duration={400} enterDelay={300}>
          {(styles) => (
            <Paper style={styles} shadow="sm" p="xl" radius="lg" withBorder>
              <Group gap="sm" mb="lg">
                <ThemeIcon size={36} radius="md" color="teal" variant="light">
                  <IconUser size={20} />
                </ThemeIcon>
                <Title order={4}>Hesap</Title>
              </Group>

              <Group gap="lg" mb="xl">
                <Avatar 
                  size={70} 
                  radius="xl" 
                  color="blue"
                  variant="gradient"
                  gradient={{ from: 'blue', to: 'cyan', deg: 90 }}
                  styles={{
                    root: {
                      boxShadow: '0 4px 20px rgba(59, 130, 246, 0.3)',
                    },
                  }}
                >
                  {user?.fullName ? getInitials(user.fullName) : 'U'}
                </Avatar>
                <Box>
                  <Text size="lg" fw={600}>{user?.fullName || 'Kullanıcı'}</Text>
                  <Text size="sm" c="dimmed">{user?.email || 'kullanici@email.com'}</Text>
                  <Badge size="sm" color="blue" variant="light" mt={4}>
                    {user?.role === 'Admin' ? 'Yönetici' : 'Kullanıcı'}
                  </Badge>
                </Box>
              </Group>

              <Stack gap="sm">
                <Button 
                  variant="light" 
                  leftSection={<IconEdit size={18} />}
                  fullWidth
                  radius="md"
                >
                  Profili Düzenle
                </Button>
                <Button 
                  variant="light" 
                  leftSection={<IconLock size={18} />}
                  fullWidth
                  radius="md"
                >
                  Şifre Değiştir
                </Button>
                <Divider my="xs" />
                <Button 
                  variant="light" 
                  color="red"
                  leftSection={<IconLogout size={18} />}
                  fullWidth
                  radius="md"
                  onClick={handleLogout}
                >
                  Çıkış Yap
                </Button>
              </Stack>
            </Paper>
          )}
        </Transition>

        {/* About */}
        <Transition mounted={mounted} transition="slide-left" duration={400} enterDelay={400}>
          {(styles) => (
            <Paper style={styles} shadow="sm" p="xl" radius="lg" withBorder>
              <Group gap="sm" mb="lg">
                <ThemeIcon size={36} radius="md" color="gray" variant="light">
                  <IconInfoCircle size={20} />
                </ThemeIcon>
                <Title order={4}>Hakkında</Title>
              </Group>

              <Stack gap="md">
                <Group justify="space-between" pb="sm" style={{ borderBottom: '1px solid var(--mantine-color-default-border)' }}>
                  <Text size="sm" c="dimmed">Versiyon</Text>
                  <Badge variant="light" color="blue">1.0.0</Badge>
                </Group>
                <Group justify="space-between" pb="sm" style={{ borderBottom: '1px solid var(--mantine-color-default-border)' }}>
                  <Text size="sm" c="dimmed">Son Güncelleme</Text>
                  <Text size="sm" fw={500}>25.12.2024</Text>
                </Group>
                <Group justify="space-between" pb="sm" style={{ borderBottom: '1px solid var(--mantine-color-default-border)' }}>
                  <Text size="sm" c="dimmed">Lisans</Text>
                  <Badge variant="light" color="green">Kurumsal</Badge>
                </Group>
                <Group justify="space-between">
                  <Text size="sm" c="dimmed">Geliştirici</Text>
                  <Text size="sm" fw={500}>Nebim A.Ş.</Text>
                </Group>
              </Stack>

              <Divider my="lg" />

              <Text size="xs" c="dimmed" ta="center">
                © 2024 Nebim Dashboard. Tüm hakları saklıdır.
              </Text>
            </Paper>
          )}
        </Transition>
      </SimpleGrid>
    </Stack>
  );
};
