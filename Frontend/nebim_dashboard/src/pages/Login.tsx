import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  TextInput,
  PasswordInput,
  Button,
  Paper,
  Title,
  Text,
  Stack,
  Alert,
  Anchor,
  Box,
  Group,
  ThemeIcon,
  Transition,
  rem,
} from '@mantine/core';
import {
  IconMail,
  IconLock,
  IconAlertCircle,
  IconChartBar,
  IconArrowRight,
  IconSparkles,
  IconShieldCheck,
  IconDeviceAnalytics,
} from '@tabler/icons-react';
import { useAuthStore } from '../store/useAuthStore';

const features = [
  {
    icon: IconDeviceAnalytics,
    title: 'Gerçek Zamanlı Analiz',
    description: 'Stok ve satış verilerinizi anlık takip edin',
  },
  {
    icon: IconSparkles,
    title: 'AI Destekli Asistan',
    description: 'Yapay zeka ile verilerinizi sorgulayın',
  },
  {
    icon: IconShieldCheck,
    title: 'Güvenli Altyapı',
    description: 'Enterprise seviye güvenlik standartları',
  },
];

export default function Login() {
  const navigate = useNavigate();
  const { login, isLoading, error, clearError } = useAuthStore();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [validationError, setValidationError] = useState('');
  const [mounted] = useState(true);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError('');
    clearError();

    if (!email.trim()) {
      setValidationError('E-posta adresi gerekli');
      return;
    }
    if (!password) {
      setValidationError('Şifre gerekli');
      return;
    }

    const success = await login(email, password);
    if (success) {
      navigate('/');
    }
  };

  return (
    <Box
      style={{
        minHeight: '100vh',
        display: 'flex',
        background: 'linear-gradient(135deg, #0c0d21 0%, #1a1b3c 50%, #0f172a 100%)',
      }}
    >
      {/* Sol Panel - Branding (hidden on mobile) */}
      <Box
        style={{
          flex: 1,
          flexDirection: 'column',
          justifyContent: 'center',
          padding: '60px',
          position: 'relative',
          overflow: 'hidden',
        }}
        display={{ base: 'none', md: 'flex' }}
      >
        {/* Animated background elements */}
        <Box
          style={{
            position: 'absolute',
            top: '10%',
            left: '10%',
            width: 300,
            height: 300,
            background: 'radial-gradient(circle, rgba(59, 130, 246, 0.15) 0%, transparent 70%)',
            borderRadius: '50%',
            filter: 'blur(40px)',
            animation: 'pulse 4s ease-in-out infinite',
          }}
        />
        <Box
          style={{
            position: 'absolute',
            bottom: '20%',
            right: '15%',
            width: 200,
            height: 200,
            background: 'radial-gradient(circle, rgba(6, 182, 212, 0.15) 0%, transparent 70%)',
            borderRadius: '50%',
            filter: 'blur(40px)',
            animation: 'pulse 5s ease-in-out infinite reverse',
          }}
        />

        <Transition mounted={mounted} transition="fade" duration={800}>
          {(styles) => (
            <Box style={{ ...styles, position: 'relative', zIndex: 1 }}>
              <Group gap="md" mb={50}>
                <ThemeIcon 
                  size={60} 
                  radius="xl" 
                  variant="gradient" 
                  gradient={{ from: 'blue', to: 'cyan', deg: 45 }}
                  style={{
                    boxShadow: '0 8px 32px rgba(59, 130, 246, 0.3)',
                  }}
                >
                  <IconChartBar size={32} />
                </ThemeIcon>
                <Box>
                  <Title order={1} c="white" fw={800} style={{ letterSpacing: '-0.5px' }}>
                    NebimFlow
                  </Title>
                  <Text c="dimmed" size="sm" tt="uppercase" style={{ letterSpacing: '2px' }}>
                    Akıllı Analitik
                  </Text>
                </Box>
              </Group>

              <Title 
                order={2} 
                c="white" 
                mb="lg" 
                style={{ fontSize: rem(42), lineHeight: 1.2 }}
              >
                İşletmenizi
                <Text 
                  component="span" 
                  inherit 
                  variant="gradient"
                  gradient={{ from: 'blue', to: 'cyan' }}
                  style={{ fontWeight: 800 }}
                >
                  {' '}Akıllıca{' '}
                </Text>
                Yönetin
              </Title>

              <Text c="dimmed" size="lg" mb={50} maw={400} lh={1.6}>
                Stok takibi, satış analizleri ve AI destekli asistan ile işletmenizi bir üst seviyeye taşıyın.
              </Text>

              <Stack gap="xl">
                {features.map((feature, index) => (
                  <Transition
                    key={feature.title}
                    mounted={mounted}
                    transition="slide-right"
                    duration={600}
                    timingFunction="ease"
                    enterDelay={200 + index * 150}
                  >
                    {(styles) => (
                      <Group 
                        style={{ 
                          ...styles,
                          padding: '16px 20px',
                          borderRadius: rem(12),
                          background: 'rgba(255, 255, 255, 0.03)',
                          border: '1px solid rgba(255, 255, 255, 0.05)',
                          backdropFilter: 'blur(10px)',
                          transition: 'transform 200ms ease, background 200ms ease',
                          cursor: 'default',
                        }}
                        gap="md"
                        onMouseEnter={(e) => {
                          e.currentTarget.style.transform = 'translateX(8px)';
                          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.05)';
                        }}
                        onMouseLeave={(e) => {
                          e.currentTarget.style.transform = 'translateX(0)';
                          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.03)';
                        }}
                      >
                        <ThemeIcon 
                          size={48} 
                          radius="lg" 
                          variant="gradient" 
                          gradient={{ from: 'blue.6', to: 'cyan.5' }}
                        >
                          <feature.icon size={24} />
                        </ThemeIcon>
                        <Box>
                          <Text c="white" fw={600} size="md">
                            {feature.title}
                          </Text>
                          <Text c="dimmed" size="sm">
                            {feature.description}
                          </Text>
                        </Box>
                      </Group>
                    )}
                  </Transition>
                ))}
              </Stack>
            </Box>
          )}
        </Transition>
      </Box>

      {/* Sağ Panel - Login Form */}
      <Box
        style={{
          width: '100%',
          maxWidth: 500,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          padding: '40px',
        }}
      >
        <Transition mounted={mounted} transition="fade-left" duration={600} enterDelay={300}>
          {(styles) => (
            <Paper 
              style={{ 
                ...styles,
                width: '100%',
                maxWidth: 400,
              }} 
              radius="xl" 
              p={40}
              shadow="xl"
              withBorder
            >
              <Title order={2} ta="center" mb={4} fw={700}>
                Hoş Geldiniz 👋
              </Title>
              <Text c="dimmed" size="sm" ta="center" mb={30}>
                Hesabınıza giriş yapın
              </Text>

              {(error || validationError) && (
                <Alert
                  icon={<IconAlertCircle size={16} />}
                  color="red"
                  mb="lg"
                  variant="light"
                  radius="md"
                >
                  {error || validationError}
                </Alert>
              )}

              <form onSubmit={handleSubmit}>
                <Stack gap="md">
                  <TextInput
                    label="E-posta"
                    placeholder="ornek@sirket.com"
                    leftSection={<IconMail size={18} stroke={1.5} />}
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    disabled={isLoading}
                    size="md"
                    radius="md"
                  />

                  <PasswordInput
                    label="Şifre"
                    placeholder="••••••••"
                    leftSection={<IconLock size={18} stroke={1.5} />}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={isLoading}
                    size="md"
                    radius="md"
                  />

                  <Group justify="flex-end">
                    <Anchor size="sm" c="dimmed">
                      Şifremi unuttum
                    </Anchor>
                  </Group>

                  <Button
                    type="submit"
                    fullWidth
                    size="lg"
                    radius="md"
                    loading={isLoading}
                    rightSection={!isLoading && <IconArrowRight size={18} />}
                    variant="gradient"
                    gradient={{ from: 'blue', to: 'cyan' }}
                    mt="md"
                    styles={{
                      root: {
                        height: rem(50),
                        transition: 'transform 150ms ease, box-shadow 150ms ease',
                        '&:hover': {
                          transform: 'translateY(-2px)',
                          boxShadow: '0 8px 20px rgba(34, 139, 230, 0.4)',
                        },
                      },
                    }}
                  >
                    Giriş Yap
                  </Button>
                </Stack>
              </form>

              <Text ta="center" mt={30} size="sm" c="dimmed">
                Hesabınız yok mu?{' '}
                <Anchor component={Link} to="/register" fw={600}>
                  Kayıt olun
                </Anchor>
              </Text>

              {/* Demo bilgisi */}
              <Paper withBorder p="md" radius="md" mt={30} bg="blue.0">
                <Text size="xs" c="blue" fw={600} mb={4}>
                  🔑 Demo Hesap
                </Text>
                <Text size="xs" c="dimmed">
                  E-posta: <strong>admin@nebim.com</strong>
                </Text>
                <Text size="xs" c="dimmed">
                  Şifre: <strong>Admin123!</strong>
                </Text>
              </Paper>
            </Paper>
          )}
        </Transition>
      </Box>

      {/* Global styles for animations */}
      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 0.5; transform: scale(1); }
          50% { opacity: 0.8; transform: scale(1.1); }
        }
      `}</style>
    </Box>
  );
}
