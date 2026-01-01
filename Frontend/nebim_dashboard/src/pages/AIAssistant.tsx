import { useState, useEffect } from 'react';
import {
  Paper,
  Title,
  Text,
  Group,
  Box,
  ThemeIcon,
  Transition,
  Badge,
  ActionIcon,
  Tooltip,
  useMantineColorScheme,
} from '@mantine/core';
import {
  IconRobot,
  IconSparkles,
  IconHistory,
  IconSettings,
} from '@tabler/icons-react';
import { ChatContainer } from '../components/ai-chat';

export const AIAssistant = () => {
  const [mounted, setMounted] = useState(false);
  const { colorScheme } = useMantineColorScheme();
  const isDark = colorScheme === 'dark';

  useEffect(() => {
    setMounted(true);
  }, []);

  return (
    <Box 
      style={{ 
        height: 'calc(100vh - 8rem)',
        display: 'flex',
        flexDirection: 'column',
        margin: '-24px',
        background: isDark 
          ? 'linear-gradient(180deg, rgba(37, 99, 235, 0.05) 0%, transparent 100%)'
          : 'linear-gradient(180deg, rgba(37, 99, 235, 0.02) 0%, transparent 100%)',
      }}
    >
      {/* Header */}
      <Transition mounted={mounted} transition="fade-down" duration={400}>
        {(styles) => (
          <Paper 
            style={{
              ...styles,
              backdropFilter: 'blur(12px)',
              background: isDark 
                ? 'rgba(26, 31, 46, 0.85)' 
                : 'rgba(255, 255, 255, 0.85)',
              borderBottom: isDark 
                ? '1px solid rgba(255, 255, 255, 0.05)' 
                : '1px solid rgba(0, 0, 0, 0.05)',
            }} 
            p={{ base: 'md', sm: 'lg' }}
            radius={0}
          >
            <Group justify="space-between" wrap="nowrap">
              <Group gap="md" wrap="nowrap">
                <ThemeIcon 
                  size={44}
                  radius="xl" 
                  variant="light" 
                  color="blue"
                  style={{
                    boxShadow: isDark 
                      ? '0 8px 32px rgba(37, 99, 235, 0.25)' 
                      : '0 8px 32px rgba(37, 99, 235, 0.15)',
                    background: isDark
                      ? 'linear-gradient(135deg, rgba(37, 99, 235, 0.2) 0%, rgba(59, 130, 246, 0.25) 100%)'
                      : 'linear-gradient(135deg, rgba(37, 99, 235, 0.1) 0%, rgba(59, 130, 246, 0.15) 100%)',
                  }}
                >
                  <IconRobot size={22} stroke={1.5} />
                </ThemeIcon>
                <Box>
                  <Group gap="sm" wrap="nowrap">
                    <Title order={4} fw={600} style={{ letterSpacing: '-0.02em' }} visibleFrom="xs">AI Asistan</Title>
                    <Title order={5} fw={600} hiddenFrom="xs">AI</Title>
                    <Badge 
                      size="sm" 
                      variant="light" 
                      color="teal"
                      radius="xl"
                      styles={{
                        root: {
                          textTransform: 'none',
                          fontWeight: 500,
                        }
                      }}
                    >
                      ● Aktif
                    </Badge>
                  </Group>
                  <Text size="sm" c="dimmed" mt={4} visibleFrom="sm">
                    NebimFlow verilerinizi doğal dille sorgulayın
                  </Text>
                </Box>
              </Group>
              
              <Group gap="sm" wrap="nowrap">
                <Tooltip label="Sohbet Geçmişi" withArrow position="bottom">
                  <ActionIcon 
                    variant="subtle" 
                    size="lg" 
                    radius="xl"
                    color="gray"
                  >
                    <IconHistory size={18} stroke={1.5} />
                  </ActionIcon>
                </Tooltip>
                <Tooltip label="Ayarlar" withArrow position="bottom">
                  <ActionIcon 
                    variant="subtle" 
                    size="lg" 
                    radius="xl"
                    color="gray"
                  >
                    <IconSettings size={18} stroke={1.5} />
                  </ActionIcon>
                </Tooltip>
              </Group>
            </Group>
          </Paper>
        )}
      </Transition>

      {/* Chat Area */}
      <Box style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
        <ChatContainer className="flex-1" />
      </Box>

      {/* Quick Tips - Hidden on mobile for cleaner UI */}
      <Transition mounted={mounted} transition="fade-up" duration={400} enterDelay={300}>
        {(styles) => (
          <Paper 
            style={{
              ...styles,
              backdropFilter: 'blur(12px)',
              background: isDark 
                ? 'rgba(26, 31, 46, 0.7)' 
                : 'rgba(255, 255, 255, 0.7)',
              borderTop: isDark 
                ? '1px solid rgba(255, 255, 255, 0.05)' 
                : '1px solid rgba(0, 0, 0, 0.05)',
            }} 
            p={{ base: 'sm', sm: 'md' }}
            radius={0}
            visibleFrom="sm"
          >
            <Group justify="center" gap="lg" wrap="wrap">
              <Group gap="sm" style={{ cursor: 'pointer' }} className="quick-tip-item">
                <Box
                  style={{
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    background: isDark 
                      ? 'linear-gradient(135deg, rgba(6, 182, 212, 0.2) 0%, rgba(6, 182, 212, 0.3) 100%)'
                      : 'linear-gradient(135deg, rgba(6, 182, 212, 0.1) 0%, rgba(6, 182, 212, 0.2) 100%)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  <IconSparkles size={14} color="#0891B2" stroke={1.5} />
                </Box>
                <Text size="xs" c="dimmed" fw={500}>"Bugünkü satışlar ne kadar?"</Text>
              </Group>
              <Group gap="sm" style={{ cursor: 'pointer' }} className="quick-tip-item">
                <Box
                  style={{
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    background: isDark 
                      ? 'linear-gradient(135deg, rgba(139, 92, 246, 0.2) 0%, rgba(139, 92, 246, 0.3) 100%)'
                      : 'linear-gradient(135deg, rgba(139, 92, 246, 0.1) 0%, rgba(139, 92, 246, 0.2) 100%)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  <IconSparkles size={14} color="#7C3AED" stroke={1.5} />
                </Box>
                <Text size="xs" c="dimmed" fw={500}>"En çok satan ürünleri göster"</Text>
              </Group>
              <Group gap="sm" style={{ cursor: 'pointer' }} className="quick-tip-item" visibleFrom="md">
                <Box
                  style={{
                    width: 28,
                    height: 28,
                    borderRadius: '50%',
                    background: isDark 
                      ? 'linear-gradient(135deg, rgba(249, 115, 22, 0.2) 0%, rgba(249, 115, 22, 0.3) 100%)'
                      : 'linear-gradient(135deg, rgba(249, 115, 22, 0.1) 0%, rgba(249, 115, 22, 0.2) 100%)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    flexShrink: 0,
                  }}
                >
                  <IconSparkles size={14} color="#EA580C" stroke={1.5} />
                </Box>
                <Text size="xs" c="dimmed" fw={500}>"Stok durumu kritik ürünler"</Text>
              </Group>
            </Group>
          </Paper>
        )}
      </Transition>

      <style>{`
        .quick-tip-item {
          transition: opacity 0.2s ease, transform 0.2s ease;
        }
        .quick-tip-item:hover {
          opacity: 0.8;
          transform: translateY(-1px);
        }
      `}</style>
    </Box>
  );
};
