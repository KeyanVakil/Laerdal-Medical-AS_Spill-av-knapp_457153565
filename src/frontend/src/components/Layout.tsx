import { ReactNode } from 'react';
import Navigation from './Navigation';

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Navigation />
      <main style={{ flex: 1, padding: '24px', maxWidth: '1280px', width: '100%', margin: '0 auto' }}>
        {children}
      </main>
    </div>
  );
}
