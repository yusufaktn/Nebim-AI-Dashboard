import { NavLink, useLocation } from 'react-router-dom';
import {
  Box,
  Text,
  Group,
  Stack,
  ThemeIcon,
  UnstyledButton,
  Paper,
  rem,
  Transition,
  ScrollArea,
  Divider,
  Badge,
} from '@mantine/core';
import {
  IconLayoutDashboard,
  IconPackage,
  IconChartBar,
  IconRobot,
  IconSettings,
  IconHelp,
  IconSparkles,
  IconChartLine,
} from '@tabler/icons-react';
import { useState, useEffect } from 'react';

interface SidebarProps {
  onClose: () => void;
}

interface NavItemProps {
  icon: React.ComponentType<{ size?: number | string; stroke?: number }>;
  label: string;
  path: string;
  badge?: string;
  badgeColor?: string;
  onClick?: () => void;
}

const navItems: NavItemProps[] = [
  { icon: IconLayoutDashboard, label: 'Dashboard', path: '/' },
  { icon: IconPackage, label: 'Stok Yönetimi', path: '/stock' },
  { icon: IconChartBar, label: 'Satış Raporu', path: '/sales', badge: 'Yakında', badgeColor: 'orange' },
  { icon: IconRobot, label: 'AI Asistan', path: '/ai', badge: 'Yeni', badgeColor: 'green' },
  { icon: IconSettings, label: 'Ayarlar', path: '/settings' },
];

const NavItem = ({ icon: Icon, label, path, badge, badgeColor, onClick }: NavItemProps) => {
  const location = useLocation();
  const isActive = location.pathname === path;
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  return (
    <Transition mounted={mounted} transition="slide-right" duration={300}>
      {(styles) => (
        <UnstyledButton
          component={NavLink}
          to={path}
          onClick={onClick}
          style={styles}
          styles={{
            root: {
              display: 'flex',
              alignItems: 'center',
              width: '100%',
              padding: 'var(--mantine-spacing-sm) var(--mantine-spacing-md)',
              borderRadius: 'var(--mantine-radius-md)',
              color: isActive 
                ? 'var(--mantine-color-blue-6)' 
                : 'var(--mantine-color-dimmed)',
              backgroundColor: isActive 
                ? 'var(--mantine-color-blue-light)' 
                : 'transparent',
              fontWeight: isActive ? 600 : 500,
              transition: 'all 150ms ease',
              '&:hover': {
                backgroundColor: isActive 
                  ? 'var(--mantine-color-blue-light)' 
                  : 'var(--mantine-color-default-hover)',
              },
            },
          }}
        >
          <Group gap="sm" style={{ flex: 1 }}>
            <ThemeIcon 
              variant={isActive ? 'filled' : 'light'} 
              color={isActive ? 'blue' : 'gray'}
              size={34}
              radius="md"
            >
              <Icon size={18} stroke={1.5} />
            </ThemeIcon>
            <Text size="sm">{label}</Text>
          </Group>
          {badge && (
            <Badge size="xs" variant="light" color={badgeColor}>
              {badge}
            </Badge>
          )}
        </UnstyledButton>
      )}
    </Transition>
  );
};

export const Sidebar = ({ onClose }: SidebarProps) => {
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  return (
    <Box h="100%" style={{ display: 'flex', flexDirection: 'column' }}>
      {/* Logo */}
      <Transition mounted={mounted} transition="fade" duration={400}>
        {(styles) => (
          <Box style={styles} p="md" pb={0}>
            <Group gap="sm">
              <ThemeIcon 
                size={42} 
                radius="xl" 
                variant="gradient" 
                gradient={{ from: 'blue', to: 'cyan', deg: 90 }}
              >
                <IconChartLine size={22} stroke={1.5} />
              </ThemeIcon>
              <Box>
                <Text size="lg" fw={700} style={{ letterSpacing: '-0.02em' }}>NebimFlow</Text>
                <Text size="xs" c="dimmed">Akıllı Analitik</Text>
              </Box>
            </Group>
          </Box>
        )}
      </Transition>

      <Divider my="md" />

      {/* Navigation */}
      <ScrollArea style={{ flex: 1 }} px="md">
        <Stack gap={4}>
          {navItems.map((item) => (
            <NavItem 
              key={item.path} 
              {...item} 
              onClick={() => {
                if (window.innerWidth < 768) {
                  onClose();
                }
              }}
            />
          ))}
        </Stack>
      </ScrollArea>

      {/* Help Section */}
      <Box p="md">
        <Transition mounted={mounted} transition="slide-up" duration={400} enterDelay={300}>
          {(styles) => (
            <Paper
              style={{
                ...styles,
                background: 'var(--mantine-color-blue-light)',
              }}
              p="md"
              radius="lg"
            >
              <Group gap="sm" mb="sm">
                <ThemeIcon 
                  size={40} 
                  radius="md" 
                  variant="gradient" 
                  gradient={{ from: 'blue', to: 'cyan' }}
                >
                  <IconSparkles size={20} />
                </ThemeIcon>
                <Box>
                  <Text size="sm" fw={600}>Yardım mı lazım?</Text>
                  <Text size="xs" c="dimmed">Dokümantasyona göz atın</Text>
                </Box>
              </Group>
              <UnstyledButton
                styles={{
                  root: {
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: rem(8),
                    width: '100%',
                    padding: 'var(--mantine-spacing-xs) var(--mantine-spacing-md)',
                    borderRadius: 'var(--mantine-radius-md)',
                    background: 'linear-gradient(90deg, var(--mantine-color-blue-6) 0%, var(--mantine-color-cyan-6) 100%)',
                    color: 'white',
                    fontWeight: 500,
                    fontSize: 'var(--mantine-font-size-sm)',
                    transition: 'transform 150ms ease, box-shadow 150ms ease',
                    '&:hover': {
                      transform: 'translateY(-1px)',
                      boxShadow: 'var(--mantine-shadow-md)',
                    },
                  },
                }}
              >
                <IconHelp size={16} />
                Yardım Merkezi
              </UnstyledButton>
            </Paper>
          )}
        </Transition>
      </Box>
    </Box>
  );
};
