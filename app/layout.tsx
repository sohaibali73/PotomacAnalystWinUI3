import type { Metadata } from 'next';
import { Inter, JetBrains_Mono } from 'next/font/google';
import './globals.css';

const inter = Inter({ subsets: ['latin'], variable: '--font-inter' });
const spaceMono = JetBrains_Mono({ subsets: ['latin'], variable: '--font-space-mono' });

export const metadata: Metadata = {
  title: 'PotomacAnalyst | API Documentation',
  description: 'Comprehensive API and integration documentation for PotomacAnalyst - the advanced financial analysis toolkit',
  viewport: {
    width: 'device-width',
    initialScale: 1,
    maximumScale: 1,
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.variable} ${spaceMono.variable} font-sans`}>
        {children}
      </body>
    </html>
  );
}
