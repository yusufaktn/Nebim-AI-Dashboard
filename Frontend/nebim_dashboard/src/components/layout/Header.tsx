import { useNavigate } from 'react-router-dom';
import {
  Group,
  Burger,
  TextInput,
  ActionIcon,
  Menu,
  Avatar,
  Text,
  Box,
  Indicator,
  Tooltip,
  rem,
  Transition,
  Divider,
  UnstyledButton,
} from '@mantine/core';
import {
  IconSearch,
  IconSun,
  IconMoon,
  IconBell,
  IconLogout,
  IconSettings,
  IconUser,
  IconChevronDown,
} from '@tabler/icons-react';
import { useAuthStore } from '../../store';
import { useAppTheme } from '../../hooks';
import { useState, useEffect } from 'react';

interface HeaderProps {
  opened: boolean;
  toggle: () => void;
}

export const Header = ({ opened, toggle }: HeaderProps) => {
  const navigate = useNavigate();
  const { isDark, toggle: toggleTheme } = useAppTheme();
  const { user, logout } = useAuthStore();
  const [mounted, setMounted] = useState(false);

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
    <Group h="100%" px="md" justify="space-between">
      {/* Left side */}
      <Group gap="md">
        <Burger opened={opened} onClick={toggle} hiddenFrom="md" size="sm" />
        
        <Transition mounted={mounted} transition="fade" duration={400}>
          {(styles) => (
            <TextInput
              style={{ ...styles, width: rem(280) }}
              placeholder="Ara..."
              leftSection={<IconSearch size={16} stroke={1.5} />}
              visibleFrom="sm"
              radius="md"
              size="sm"
              styles={{
                input: {
                  backgroundColor: isDark 
                    ? 'var(--mantine-color-dark-6)' 
                    : 'var(--mantine-color-gray-0)',
                  border: '1px solid transparent',
                  transition: 'all 150ms ease',
                  '&:focus': {
                    borderColor: 'var(--mantine-color-blue-5)',
                    boxShadow: '0 0 0 2px var(--mantine-color-blue-1)',
                  },
                },
              }}
            />
          )}
        </Transition>
      </Group>

      {/* Right side */}
      <Transition mounted={mounted} transition="fade" duration={400} enterDelay={100}>
        {(styles) => (
          <Group style={styles} gap="xs">
            {/* Theme Toggle */}
            <Tooltip label={isDark ? 'Açık tema' : 'Koyu tema'}>
              <ActionIcon
                variant="light"
                size="lg"
                radius="md"
                onClick={() => toggleTheme()}
                styles={{
                  root: {
                    transition: 'transform 150ms ease',
                    '&:hover': {
                      transform: 'scale(1.05)',
                    },
                  },
                }}
              >
                {isDark ? (
                  <IconSun size={18} stroke={1.5} />
                ) : (
                  <IconMoon size={18} stroke={1.5} />
                )}
              </ActionIcon>
            </Tooltip>

            {/* Notifications */}
            <Tooltip label="Bildirimler">
              <Indicator color="red" size={8} offset={4} processing>
                <ActionIcon
                  variant="light"
                  size="lg"
                  radius="md"
                  styles={{
                    root: {
                      transition: 'transform 150ms ease',
                      '&:hover': {
                        transform: 'scale(1.05)',
                      },
                    },
                  }}
                >
                  <IconBell size={18} stroke={1.5} />
                </ActionIcon>
              </Indicator>
            </Tooltip>

            <Divider orientation="vertical" mx="xs" />

            {/* User Menu */}
            <Menu 
              position="bottom-end" 
              withArrow 
              width={220}
              shadow="lg"
              radius="md"
            >
              <Menu.Target>
                <UnstyledButton
                  styles={{
                    root: {
                      display: 'flex',
                      alignItems: 'center',
                      gap: rem(10),
                      padding: 'var(--mantine-spacing-xs) var(--mantine-spacing-sm)',
                      borderRadius: 'var(--mantine-radius-md)',
                      transition: 'background-color 150ms ease',
                      '&:hover': {
                        backgroundColor: 'var(--mantine-color-default-hover)',
                      },
                    },
                  }}
                >
                  <Avatar 
                    size={38} 
                    radius="md" 
                    color="blue"
                    variant="gradient"
                    gradient={{ from: 'blue', to: 'cyan', deg: 90 }}
                  >
                    {user?.fullName ? getInitials(user.fullName) : 'U'}
                  </Avatar>
                  <Box visibleFrom="sm">
                    <Text size="sm" fw={500} lh={1.2}>
                      {user?.fullName || 'Kullanıcı'}
                    </Text>
                    <Text size="xs" c="dimmed" lh={1.2}>
                      {user?.role === 'Admin' ? 'Yönetici' : 'Kullanıcı'}
                    </Text>
                  </Box>
                  <IconChevronDown size={14} stroke={1.5} color="var(--mantine-color-dimmed)" />
                </UnstyledButton>
              </Menu.Target>

              <Menu.Dropdown>
                <Menu.Label>Hesap</Menu.Label>
                <Menu.Item 
                  leftSection={<IconUser size={16} stroke={1.5} />}
                  onClick={() => navigate('/settings')}
                >
                  Profil
                </Menu.Item>
                <Menu.Item 
                  leftSection={<IconSettings size={16} stroke={1.5} />}
                  onClick={() => navigate('/settings')}
                >
                  Ayarlar
                </Menu.Item>
                <Menu.Divider />
                <Menu.Item 
                  leftSection={<IconLogout size={16} stroke={1.5} />}
                  onClick={handleLogout}
                  color="red"
                >
                  Çıkış Yap
                </Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Group>
        )}
      </Transition>
    </Group>
  );
};
