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
  Progress,
  rem,
  List,
} from '@mantine/core';
import {
  IconMail,
  IconLock,
  IconUser,
  IconAlertCircle,
  IconChartBar,
  IconArrowRight,
  IconCheck,
  IconX,
} from '@tabler/icons-react';
import { useAuthStore } from '../store/useAuthStore';

// Password requirement checker
function PasswordRequirement({ meets, label }: { meets: boolean; label: string }) {
  return (
    <Text c={meets ? 'teal' : 'red'} size="xs" style={{ display: 'flex', alignItems: 'center', gap: 7 }}>
      {meets ? <IconCheck size={14} stroke={2} /> : <IconX size={14} stroke={2} />}
      {label}
    </Text>
  );
}

const requirements = [
  { re: /.{6,}/, label: 'En az 6 karakter' },
  { re: /[0-9]/, label: 'En az bir rakam' },
  { re: /[a-z]/, label: 'En az bir küçük harf' },
  { re: /[A-Z]/, label: 'En az bir büyük harf' },
];

function getStrength(password: string) {
  let multiplier = password.length > 5 ? 0 : 1;

  requirements.forEach((requirement) => {
    if (!requirement.re.test(password)) {
      multiplier += 1;
    }
  });

  return Math.max(100 - (100 / (requirements.length + 1)) * multiplier, 0);
}

export default function Register() {
  const navigate = useNavigate();
  const { register, isLoading, error, clearError } = useAuthStore();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [validationError, setValidationError] = useState('');
  const [mounted] = useState(true);

  const strength = getStrength(password);
  const checks = requirements.map((requirement, index) => (
    <PasswordRequirement key={index} label={requirement.label} meets={requirement.re.test(password)} />
  ));

  const getStrengthColor = (strength: number) => {
    if (strength < 30) return 'red';
    if (strength < 60) return 'orange';
    if (strength < 80) return 'yellow';
    return 'teal';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setValidationError('');
    clearError();

    if (!fullName.trim()) {
      setValidationError('Ad Soyad gerekli');
      return;
    }
    if (!email.trim()) {
      setValidationError('E-posta adresi gerekli');
      return;
    }
    if (!password) {
      setValidationError('Şifre gerekli');
      return;
    }
    if (strength < 60) {
      setValidationError('Daha güçlü bir şifre belirleyin');
      return;
    }
    if (password !== confirmPassword) {
      setValidationError('Şifreler eşleşmiyor');
      return;
    }

    const success = await register(email, password, fullName);
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
          alignItems: 'center',
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
            top: '20%',
            left: '20%',
            width: 350,
            height: 350,
            background: 'radial-gradient(circle, rgba(139, 92, 246, 0.15) 0%, transparent 70%)',
            borderRadius: '50%',
            filter: 'blur(40px)',
            animation: 'float 6s ease-in-out infinite',
          }}
        />
        <Box
          style={{
            position: 'absolute',
            bottom: '15%',
            right: '20%',
            width: 250,
            height: 250,
            background: 'radial-gradient(circle, rgba(6, 182, 212, 0.15) 0%, transparent 70%)',
            borderRadius: '50%',
            filter: 'blur(40px)',
            animation: 'float 5s ease-in-out infinite reverse',
          }}
        />

        <Transition mounted={mounted} transition="fade" duration={800}>
          {(styles) => (
            <Box style={{ ...styles, textAlign: 'center', position: 'relative', zIndex: 1 }}>
              <Group gap="md" justify="center" mb={40}>
                <ThemeIcon 
                  size={70} 
                  radius="xl" 
                  variant="gradient" 
                  gradient={{ from: 'violet', to: 'cyan', deg: 45 }}
                  style={{
                    boxShadow: '0 8px 32px rgba(139, 92, 246, 0.3)',
                  }}
                >
                  <IconChartBar size={36} />
                </ThemeIcon>
              </Group>

              <Title 
                order={1} 
                c="white" 
                mb="md" 
                style={{ fontSize: rem(48), fontWeight: 800 }}
              >
                NebimFlow
              </Title>

              <Text c="dimmed" size="xl" mb={40} maw={450}>
                İş süreçlerinizi optimize edin, verilerinizi analiz edin ve 
                <Text component="span" c="cyan" fw={600}> yapay zeka </Text>
                ile kararlarınızı güçlendirin.
              </Text>

              <Paper
                p="xl"
                radius="lg"
                style={{
                  background: 'rgba(255, 255, 255, 0.03)',
                  border: '1px solid rgba(255, 255, 255, 0.05)',
                  backdropFilter: 'blur(10px)',
                }}
              >
                <List
                  spacing="md"
                  size="md"
                  c="dimmed"
                  icon={
                    <ThemeIcon size={24} radius="xl" variant="light" color="cyan">
                      <IconCheck size={14} />
                    </ThemeIcon>
                  }
                >
                  <List.Item>Sınırsız kullanıcı ve veri</List.Item>
                  <List.Item>7/24 teknik destek</List.Item>
                  <List.Item>Otomatik yedekleme</List.Item>
                  <List.Item>SSL güvenlik sertifikası</List.Item>
                </List>
              </Paper>
            </Box>
          )}
        </Transition>
      </Box>

      {/* Sağ Panel - Register Form */}
      <Box
        style={{
          width: '100%',
          maxWidth: 520,
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
                maxWidth: 440,
              }} 
              radius="xl" 
              p={40}
              shadow="xl"
              withBorder
            >
              <Title order={2} ta="center" mb={4} fw={700}>
                Hesap Oluştur 🚀
              </Title>
              <Text c="dimmed" size="sm" ta="center" mb={30}>
                Hemen ücretsiz hesabınızı oluşturun
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
                    label="Ad Soyad"
                    placeholder="Adınız Soyadınız"
                    leftSection={<IconUser size={18} stroke={1.5} />}
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    disabled={isLoading}
                    size="md"
                    radius="md"
                  />

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

                  <div>
                    <PasswordInput
                      label="Şifre"
                      placeholder="Güçlü bir şifre belirleyin"
                      leftSection={<IconLock size={18} stroke={1.5} />}
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      disabled={isLoading}
                      size="md"
                      radius="md"
                    />
                    {password.length > 0 && (
                      <Box mt="xs">
                        <Progress 
                          value={strength} 
                          color={getStrengthColor(strength)} 
                          size="xs" 
                          radius="xl"
                          mb="xs"
                        />
                        <Group gap="xs">
                          {checks}
                        </Group>
                      </Box>
                    )}
                  </div>

                  <PasswordInput
                    label="Şifre Tekrar"
                    placeholder="Şifrenizi tekrar girin"
                    leftSection={<IconLock size={18} stroke={1.5} />}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    disabled={isLoading}
                    size="md"
                    radius="md"
                    error={confirmPassword && password !== confirmPassword ? 'Şifreler eşleşmiyor' : null}
                  />

                  <Button
                    type="submit"
                    fullWidth
                    size="lg"
                    radius="md"
                    loading={isLoading}
                    rightSection={!isLoading && <IconArrowRight size={18} />}
                    variant="gradient"
                    gradient={{ from: 'violet', to: 'cyan' }}
                    mt="md"
                    styles={{
                      root: {
                        height: rem(50),
                        transition: 'transform 150ms ease, box-shadow 150ms ease',
                        '&:hover': {
                          transform: 'translateY(-2px)',
                          boxShadow: '0 8px 20px rgba(139, 92, 246, 0.4)',
                        },
                      },
                    }}
                  >
                    Kayıt Ol
                  </Button>
                </Stack>
              </form>

              <Text ta="center" mt={30} size="sm" c="dimmed">
                Zaten hesabınız var mı?{' '}
                <Anchor component={Link} to="/login" fw={600}>
                  Giriş yapın
                </Anchor>
              </Text>

              <Text ta="center" mt="lg" size="xs" c="dimmed">
                Kayıt olarak{' '}
                <Anchor size="xs">Kullanım Şartları</Anchor>
                {' '}ve{' '}
                <Anchor size="xs">Gizlilik Politikası</Anchor>
                'nı kabul etmiş olursunuz.
              </Text>
            </Paper>
          )}
        </Transition>
      </Box>

      {/* Global styles for animations */}
      <style>{`
        @keyframes float {
          0%, 100% { transform: translateY(0) rotate(0deg); }
          50% { transform: translateY(-20px) rotate(5deg); }
        }
      `}</style>
    </Box>
  );
}
