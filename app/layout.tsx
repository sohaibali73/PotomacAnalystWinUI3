import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';

const inter = Inter({ 
  subsets: ['latin'], 
  variable: '--font-inter',
  display: 'swap',
});

export const metadata: Metadata = {
  title: 'PotomacAnalyst | Documentation',
  description: 'The advanced financial analysis toolkit for modern analysts',
};

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  maximumScale: 1,
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="bg-white" suppressHydrationWarning>
      <body className={`${inter.variable} font-sans bg-white text-[#1d1d1f] antialiased`}>
        {children}
      </body>
    </html>
  );
}
