import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './app/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        'surface-primary': 'hsl(var(--surface-primary))',
        'surface-secondary': 'hsl(var(--surface-secondary))',
        'text-primary': 'hsl(var(--text-primary))',
        'text-secondary': 'hsl(var(--text-secondary))',
        'text-muted': 'hsl(var(--text-muted))',
        'accent-primary': 'hsl(var(--accent-primary))',
        'accent-secondary': 'hsl(var(--accent-secondary))',
        'border-color': 'hsl(var(--border-color))',
      },
      fontFamily: {
        sans: ['var(--font-inter)'],
        mono: ['var(--font-space-mono)'],
      },
    },
  },
  plugins: [],
};

export default config;
