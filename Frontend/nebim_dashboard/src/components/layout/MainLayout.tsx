import { Outlet } from 'react-router-dom';
import { AppShell } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { Sidebar } from './Sidebar';
import { Header } from './Header';
import { useAppTheme } from '../../hooks';

export const MainLayout = () => {
  const [opened, { toggle }] = useDisclosure();
  const { isDark } = useAppTheme();

  return (
    <AppShell
      header={{ height: 70 }}
      navbar={{ width: 280, breakpoint: 'md', collapsed: { mobile: !opened } }}
      padding="md"
      styles={{
        main: {
          backgroundColor: isDark 
            ? 'var(--mantine-color-dark-8)' 
            : 'var(--mantine-color-gray-0)',
          minHeight: '100vh',
        },
      }}
    >
      <AppShell.Header>
        <Header opened={opened} toggle={toggle} />
      </AppShell.Header>

      <AppShell.Navbar>
        <Sidebar onClose={() => opened && toggle()} />
      </AppShell.Navbar>

      <AppShell.Main>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
};
